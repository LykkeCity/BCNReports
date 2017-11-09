using AzureRepositories.ReportStorage;
using AzureStorage;
using Common.Log;
using Lykke.Service.BcnReports.Core.ReportStorage;

namespace Lykke.Service.BcnReports.AzureRepositories.ReportStorage
{
    public class AssetTransactionsReportStorage : BaseReportStorage, IAssetTransactionsReportStorage
    {
        private const string Container = "asset-transaction-reports";

        public AssetTransactionsReportStorage(ILog log, IBlobStorage blobStorage) : base(Container, log, blobStorage)
        {
        }
    }
}
