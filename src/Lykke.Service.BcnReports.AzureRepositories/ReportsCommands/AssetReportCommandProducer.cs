using System.Threading.Tasks;
using AzureStorage.Queue;
using Common;
using Lykke.Service.BcnReports.Core.Queue;

namespace Lykke.Service.BcnReports.AzureRepositories.ReportsCommands
{
    public class AssetReportCommandProducer: IAssetReportCommandProducer
    {
        private readonly IQueueExt _queue;

        public AssetReportCommandProducer(IQueueExt queue)
        {
            _queue = queue;
        }

        public async Task CreateCommand(string assetId, string email)
        {
            var msg = new AssetTransactionReportQueueCommand
            {
                AssetId = assetId,
                Email = email
            };

            await _queue.PutRawMessageAsync(msg.ToJson());
        }
    }
}
