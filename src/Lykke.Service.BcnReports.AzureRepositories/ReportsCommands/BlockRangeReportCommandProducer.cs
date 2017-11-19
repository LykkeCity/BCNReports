using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AzureStorage.Queue;
using Common;
using Lykke.Service.BcnReports.Core.Queue;

namespace Lykke.Service.BcnReports.AzureRepositories.ReportsCommands
{
    public class BlockRangeReportCommandProducer : IBlockRangeReportCommandProducer
    {
        private readonly IQueueExt _queue;

        public BlockRangeReportCommandProducer(IQueueExt queue)
        {
            _queue = queue;
        }
        
        public async Task CreateCommand(IEnumerable<string> blocks, string email)
        {
            var msg = new BlockTransactionReportQueueCommand
            {
                Blocks = blocks.ToArray(),
                Email = email
            };

            await _queue.PutRawMessageAsync(msg.ToJson());
        }

        public async Task CreateRangeReport(int fromBlock, int toBlock, int batch)
        {
            var msg = new BlockTransactionRangeReportQueueCommand
            {
                From = fromBlock,
                To = toBlock,
                Batch = batch
            };

            await _queue.PutRawMessageAsync(msg.ToJson());
        }
    }
}
