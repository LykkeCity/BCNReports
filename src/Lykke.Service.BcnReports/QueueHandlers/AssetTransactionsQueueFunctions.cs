using System;
using System.Threading.Tasks;
using Common;
using Common.Log;
using Lykke.JobTriggers.Triggers.Attributes;
using Lykke.Service.BcnReports.Core.AssetTransactionReport;
using Lykke.Service.BcnReports.Core.Queue;
using Lykke.Service.BcnReports.Core.ReportMetadata;
using Lykke.Service.BcnReports.Core.ReportStorage;
using Lykke.Service.EmailSender;

namespace Lykke.Service.BcnReports.QueueHandlers
{
    public class AssetTransactionsQueueFunctions
    {
        private readonly IAssetTransactionsReportService _reportService;
        private readonly IAssetTransactionsReportMetadataRepository _metadataRepository;
        private readonly ILog _log;
        private readonly IAssetTransactionsReportStorage _reportStorage;
        private readonly EmailSenderClient _emailSenderProducer;

        public AssetTransactionsQueueFunctions(EmailSenderClient emailSenderProducer, 
            IAssetTransactionsReportService reportService, 
            IAssetTransactionsReportMetadataRepository metadataRepository, 
            ILog log, 
            IAssetTransactionsReportStorage reportStorage)
        {
            _emailSenderProducer = emailSenderProducer;
            _reportService = reportService;
            _metadataRepository = metadataRepository;
            _log = log;
            _reportStorage = reportStorage;
        }

        [QueueTrigger(QueueNames.AssetTransactionsReport, notify:true)]
        public async Task CreateReport(AssetTransactionReportQueueCommand command)
        {
            try
            {
                await _log.WriteInfoAsync(nameof(AssetTransactionsQueueFunctions),
                    nameof(CreateReport),
                    command.ToJson(), "Started");

                await _metadataRepository.SetProcessing(command.AssetId);
                var reportDate = DateTime.UtcNow;

                var reportData = await _reportService.GetTransactionsReport(command.AssetId);


                var saveResult = await _reportStorage.SaveXlsxReport(command.AssetId, reportData);

                var emailMes = new EmailMessage
                {
                    Subject = $"Report for assetId {command.AssetId} at {reportDate:f}",
                    TextBody = $"Report for assetId {command.AssetId} at {reportDate:f} - {saveResult.Url}",
                };

                if (!string.IsNullOrEmpty(command.Email))
                {
                    await _emailSenderProducer.SendAsync(emailMes, new EmailAddressee(){DisplayName = command.Email, EmailAddress = command.Email});
                }

                await _metadataRepository.SetDone(command.AssetId, saveResult.Url);

                await _log.WriteInfoAsync(nameof(AssetTransactionsQueueFunctions),
                    nameof(CreateReport),
                    command.ToJson(), "Done");
            }
            catch (Exception e)
            {
                await _log.WriteFatalErrorAsync(nameof(AssetTransactionsQueueFunctions), 
                    nameof(CreateReport),
                    command.ToJson(), e);

                await _metadataRepository.SetError(command.AssetId, e.ToString());
                throw;
            }
        }
    }
}
