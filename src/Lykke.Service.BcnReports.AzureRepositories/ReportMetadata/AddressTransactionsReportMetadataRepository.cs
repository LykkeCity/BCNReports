using AzureStorage;
using Lykke.Service.BcnReports.Core.ReportMetadata;

namespace Lykke.Service.BcnReports.AzureRepositories.ReportMetadata
{

    public class AddressTransactionsReportMetadataRepository: BaseReportMetadataRepository, IAddressTransactionsReportMetadataRepository
    {
        public AddressTransactionsReportMetadataRepository(INoSQLTableStorage<BaseReportMetadataEntity> storage) : base(storage)
        {
        }
    }
}
