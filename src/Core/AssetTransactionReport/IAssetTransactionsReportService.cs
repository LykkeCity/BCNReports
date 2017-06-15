using System.IO;
using System.Threading.Tasks;

namespace Core.AssetTransactionReport
{
    public interface IAssetTransactionsReportService
    {
        Task<Stream> GetTransactionsReport(string assetId);
    }
}
