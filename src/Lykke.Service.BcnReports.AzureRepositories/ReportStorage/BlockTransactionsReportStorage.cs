using AzureRepositories.ReportStorage;
using AzureStorage;
using Common.Log;
using Lykke.Service.BcnReports.Core.ReportStorage;

namespace Lykke.Service.BcnReports.AzureRepositories.ReportStorage
{
    public class BlockTransactionsReportStorage : BaseReportStorage, IBlockTransactionsReportStorage
    {
        private const string Container = "block-transaction-reports";

        public BlockTransactionsReportStorage(ILog log, IBlobStorage blobStorage) : base(Container, log, blobStorage)
        {
        }
    }
}
