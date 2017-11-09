using System;
using System.Linq;
using System.Threading.Tasks;
using Common;
using Common.Log;
using Lykke.JobTriggers.Triggers.Attributes;
using Lykke.Service.BcnReports.AzureRepositories.Helpers;
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
                
                var reportDate = DateTime.UtcNow;

                var saveResults = await command.Blocks.SelectAsync(p=> Retry.Try(()=> SaveReport(p), nameof(CreateReport), 5, logger:_log));


                if (!string.IsNullOrEmpty(command.Email))
                {

                    var reportDescrpt = saveResults.Select(p => $"Report for blocks at {reportDate:f} - {p.url}");
                    var emailMes = new EmailMessage
                    {
                        Subject = $"Report for block {command.Blocks} at {reportDate:f}",
                        HtmlBody = string.Join("<br/>", reportDescrpt)
                    };

                    await _emailSenderProducer.SendAsync(emailMes, new EmailAddressee{DisplayName = command.Email, EmailAddress = command.Email});
                }

                await _log.WriteMonitorAsync(nameof(BlockTransactionsQueueFunctions),
                    nameof(CreateReport),
                    command.ToJson(), "Done");
            }
            catch (Exception e)
            {
                await _log.WriteErrorAsync(nameof(BlockTransactionsQueueFunctions), 
                    nameof(CreateReport),
                    command.ToJson(), e);
                throw;
            }
        }

        private async Task<(string block, string url)> SaveReport(string block)
        {
            try
            {
                await _log.WriteMonitorAsync(nameof(SaveReport),
                    nameof(SaveReport),
                    block, "Started");

                await _metadataRepository.SetProcessing(block);

                var meta = await _metadataRepository.Get(block);

                if (meta?.FileUrl != null)
                {
                    await _metadataRepository.SetDone(block, meta.FileUrl);
                    return (block, meta.FileUrl);
                }
                var reportData = await _reportService.GetTransactionsReport(block);


                var saveResult = await _reportStorage.SaveXlsxReport(block, reportData);


                await _metadataRepository.SetDone(block, saveResult.Url);

                await _log.WriteMonitorAsync(nameof(BlockTransactionsQueueFunctions),
                    nameof(SaveReport),
                    block, "Done");

                return (block, saveResult.Url);
            }
            catch (Exception e)
            {
                await _log.WriteErrorAsync(nameof(BlockTransactionsQueueFunctions),
                    nameof(CreateReport),
                    block, e);

                await _metadataRepository.SetError(block, e.ToString());
                throw;
            }
        }
    }
}
