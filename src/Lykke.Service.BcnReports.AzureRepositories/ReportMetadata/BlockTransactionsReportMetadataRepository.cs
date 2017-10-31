using System.Threading.Tasks;
using AzureStorage;
using Lykke.Service.BcnReports.Core.ReportMetadata;

namespace Lykke.Service.BcnReports.AzureRepositories.ReportMetadata
{

    public class BlockTransactionsReportMetadataRepository: BaseReportMetadataRepository, IBlockTransactionsReportMetadataRepository
    {
        public BlockTransactionsReportMetadataRepository(INoSQLTableStorage<BaseReportMetadataEntity> storage) : base(storage)
        {
        }

        public override async Task InsertOrReplace(IBaseReportMetadata reportMetadata)
        {

            var item = await _storage.GetDataAsync(BaseReportMetadataEntity.GeneratePartitionKey(),
                BaseReportMetadataEntity.GenerateRowKey(reportMetadata.Id));
            reportMetadata.FileUrl = item?.FileUrl;

            await base.InsertOrReplace(reportMetadata);
        }
    }
}
