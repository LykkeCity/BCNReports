using AzureStorage;
using Lykke.Service.BcnReports.AzureRepositories.ReportMetadata;
using Lykke.Service.BcnReports.Core.ReportMetadata;

namespace AzureRepositories.ReportMetadata
{

    public class AssetTransactionsReportMetadataRepository: BaseReportMetadataRepository, IAssetTransactionsReportMetadataRepository
    {
        public AssetTransactionsReportMetadataRepository(INoSQLTableStorage<BaseReportMetadataEntity> storage) : base(storage)
        {
        }
    }
}
