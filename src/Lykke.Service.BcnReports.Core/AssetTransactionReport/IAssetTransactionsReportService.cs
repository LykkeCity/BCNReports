using System.IO;
using System.Threading.Tasks;

namespace Lykke.Service.BcnReports.Core.AssetTransactionReport
{
    public interface IAssetTransactionsReportService
    {
        Task<Stream> GetTransactionsReport(string assetId);
    }
}
