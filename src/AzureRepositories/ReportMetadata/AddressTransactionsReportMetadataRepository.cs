using AzureStorage;
using Core.ReportMetadata;

namespace AzureRepositories.ReportMetadata
{

    public class AddressTransactionsReportMetadataRepository: BaseReportMetadataRepository, IAddressTransactionsReportMetadataRepository
    {
        public AddressTransactionsReportMetadataRepository(INoSQLTableStorage<BaseReportMetadataEntity> storage) : base(storage)
        {
        }
    }
}
