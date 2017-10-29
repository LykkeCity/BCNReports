using AzureStorage;
using Lykke.Service.BcnReports.Core.ReportMetadata;

namespace Lykke.Service.BcnReports.AzureRepositories.ReportMetadata
{

    public class BlockTransactionsReportMetadataRepository: BaseReportMetadataRepository, IBlockTransactionsReportMetadataRepository
    {
        public BlockTransactionsReportMetadataRepository(INoSQLTableStorage<BaseReportMetadataEntity> storage) : base(storage)
        {
        }
    }
}
