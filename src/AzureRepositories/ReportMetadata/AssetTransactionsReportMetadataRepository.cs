using AzureStorage;
using Core.ReportMetadata;

namespace AzureRepositories.ReportMetadata
{

    public class AssetTransactionsReportMetadataRepository: BaseReportMetadataRepository, IAssetTransactionsReportMetadataRepository
    {
        public AssetTransactionsReportMetadataRepository(INoSQLTableStorage<BaseReportMetadataEntity> storage) : base(storage)
        {
        }
    }
}
