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

        public async Task CreateCommand(string blockId, string email)
        {
            var msg = new BlockTransactionReportQueueCommand
            {
                BlockId = blockId,
                Email = email
            };

            await _queue.PutRawMessageAsync(msg.ToJson());
        }
    }
}
