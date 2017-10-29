using System;
using System.Threading.Tasks;
using Common;
using Common.Log;
using Lykke.JobTriggers.Triggers.Attributes;
using Lykke.Service.BcnReports.Core.BlockTransactionsReport;
using Lykke.Service.BcnReports.Core.Queue;
using Lykke.Service.BcnReports.Core.ReportMetadata;
using Lykke.Service.BcnReports.Core.ReportStorage;
using Lykke.Service.EmailSender;

namespace Lykke.Service.BcnReports.QueueHandlers
{
    public class BlockTransactionsQueueFunctions
    {
        private readonly IBlockTransactionsReportService _reportService;
        private readonly IBlockTransactionsReportMetadataRepository _metadataRepository;
        private readonly ILog _log;
        private readonly IBlockTransactionsReportStorage _reportStorage;
        private readonly EmailSenderClient _emailSenderProducer;

        public BlockTransactionsQueueFunctions(EmailSenderClient emailSenderProducer,
            IBlockTransactionsReportService reportService, 
            IBlockTransactionsReportMetadataRepository metadataRepository, 
            ILog log,
            IBlockTransactionsReportStorage reportStorage)
        {
            _emailSenderProducer = emailSenderProducer;
            _reportService = reportService;
            _metadataRepository = metadataRepository;
            _log = log;
            _reportStorage = reportStorage;
        }

        [QueueTrigger(QueueNames.BlockTransactionsReport, notify:true)]
        public async Task CreateReport(BlockTransactionReportQueueCommand command)
        {
            try
            {
                await _log.WriteMonitorAsync(nameof(BlockTransactionsQueueFunctions),
                    nameof(CreateReport),
                    command.ToJson(), "Started");

                await _metadataRepository.SetProcessing(command.BlockId);
                var reportDate = DateTime.UtcNow;

                var reportData = await _reportService.GetTransactionsReport(command.BlockId);


                var saveResult = await _reportStorage.Save(command.BlockId, reportData);

                var emailMes = new EmailMessage
                {
                    Subject = $"Report for block {command.BlockId} at {reportDate:f}",
                    TextBody = $"Report for block {command.BlockId} at {reportDate:f} - {saveResult.Url}",
                };

                if (!string.IsNullOrEmpty(command.Email))
                {
                    await _emailSenderProducer.SendAsync(emailMes, new EmailAddressee{DisplayName = command.Email, EmailAddress = command.Email});
                }

                await _metadataRepository.SetDone(command.BlockId, saveResult.Url);

                await _log.WriteMonitorAsync(nameof(BlockTransactionsQueueFunctions),
                    nameof(CreateReport),
                    command.ToJson(), "Done");
            }
            catch (Exception e)
            {
                await _log.WriteFatalErrorAsync(nameof(BlockTransactionsQueueFunctions), 
                    nameof(CreateReport),
                    command.ToJson(), e);

                await _metadataRepository.SetError(command.BlockId, e.ToString());
                throw;
            }
        }
    }
}
