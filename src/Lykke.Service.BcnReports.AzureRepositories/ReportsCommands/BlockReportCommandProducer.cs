using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AzureStorage.Queue;
using Common;
using Lykke.Service.BcnReports.Core.Queue;

namespace Lykke.Service.BcnReports.AzureRepositories.ReportsCommands
{
    public class BlockReportCommandProducer: IBlockReportCommandProducer
    {
        private readonly IQueueExt _queue;

        public BlockReportCommandProducer(IQueueExt queue)
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
    }
}
