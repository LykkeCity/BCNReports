using System.Threading.Tasks;

namespace Lykke.Service.BcnReports.Core.Queue
{
    public class AssetTransactionReportQueueCommand
    {
        public string AssetId { get; set; }

        public string Email { get; set; }
    }

    public interface IAssetReportCommandProducer
    {
        Task CreateCommand(string assetId, string email);
    }
}
