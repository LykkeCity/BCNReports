using System.Collections.Generic;
using System.Threading.Tasks;

namespace Lykke.Service.BcnReports.Core.Asset
{
    public interface IAssetTransaction
    {
        string TransactionId { get; }
    }
    public interface IAssetTransactionsService
    {
        Task<IEnumerable<IAssetTransaction>> GetTransactionsForAsset(string assetId);
    }
}
