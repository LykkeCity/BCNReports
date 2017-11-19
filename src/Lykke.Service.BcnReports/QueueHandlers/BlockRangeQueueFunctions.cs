using System;
using System.Linq;
using System.Threading.Tasks;
using Common;
using Common.Log;
using Lykke.JobTriggers.Triggers.Attributes;
using Lykke.Service.BcnReports.AzureRepositories.Helpers;
using Lykke.Service.BcnReports.Core.BlockTransactionsReport;
using Lykke.Service.BcnReports.Core.Console;
using Lykke.Service.BcnReports.Core.Queue;
using Lykke.Service.BcnReports.Core.ReportMetadata;
using Lykke.Service.BcnReports.Core.ReportStorage;
using Lykke.Service.EmailSender;
using MoreLinq;

namespace Lykke.Service.BcnReports.QueueHandlers
{
    public class BlockRangeQueueFunctions
    {
        private readonly IConsole _console;
        private readonly IBlockReportCommandProducer _commandProducer;
        private readonly IBlockTransactionsReportMetadataRepository _reportMetadataRepository;

        public BlockRangeQueueFunctions(IConsole console, IBlockReportCommandProducer commandProducer, IBlockTransactionsReportMetadataRepository reportMetadataRepository)
        {
            _console = console;
            _commandProducer = commandProducer;
            _reportMetadataRepository = reportMetadataRepository;
        }


        [QueueTrigger(QueueNames.BlockTransactionsRangeReport, notify:true)]
        public async Task PrepareReportCommands(BlockTransactionRangeReportQueueCommand command)
        {
            _console.WriteConsoleLog(nameof(BlockRangeQueueFunctions), nameof(PrepareReportCommands), command.ToJson(), "Started");
            var list = Enumerable.Range(command.From, command.To - command.From + 1);

            foreach (var batch in list.Batch(command.Batch).ToList())
            {

                var insertInTableTasks = batch.Select(block => Retry.Try(() => _reportMetadataRepository.InsertOrReplace(ReportMetadata.Create(
                    block.ToString(),
                    queuedAt: DateTime.UtcNow)), nameof(PrepareReportCommands), 30, secondsToWaitOnFail: 2));
                //foreach (var block in batch)
                //{
                //    await Retry.Try(() => _reportMetadataRepository.InsertOrReplace(ReportMetadata.Create(block.ToString(),
                //            queuedAt: DateTime.UtcNow)), nameof(PrepareReportCommands), 30, secondsToWaitOnFail:2);
                //}

                await Task.WhenAll(insertInTableTasks);
                await _commandProducer.CreateCommand(batch.Select(p => p.ToString()), null);
            }

            _console.WriteConsoleLog(nameof(BlockRangeQueueFunctions), nameof(PrepareReportCommands), command.ToJson(), "Done");
        }
        
    }
}
