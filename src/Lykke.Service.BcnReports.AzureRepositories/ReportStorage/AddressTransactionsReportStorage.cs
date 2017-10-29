using AzureRepositories.ReportStorage;
using AzureStorage;
using Common.Log;
using Lykke.Service.BcnReports.Core.ReportStorage;

namespace Lykke.Service.BcnReports.AzureRepositories.ReportStorage
{
    public class AddressTransactionsReportStorage : BaseReportStorage, IAddressTransactionsReportStorage
    {
        private const string Container = "address-transaction-reports";

        public AddressTransactionsReportStorage(ILog log, IBlobStorage blobStorage) : base(Container, log, blobStorage)
        {
        }
    }
}
