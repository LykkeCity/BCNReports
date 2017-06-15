using AzureStorage;
using Common.Log;
using Core.ReportStorage;

namespace AzureRepositories.ReportStorage
{
    public class AssetTransactionsReportStorage : BaseReportStorage, IAssetTransactionsReportStorage
    {
        private const string Container = "asset-transaction-reports";

        public AssetTransactionsReportStorage(ILog log, IBlobStorage blobStorage) : base(Container, log, blobStorage)
        {
        }
    }
}
