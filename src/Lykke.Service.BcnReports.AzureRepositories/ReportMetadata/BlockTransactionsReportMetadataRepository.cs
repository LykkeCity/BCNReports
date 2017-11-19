using System.Collections.Generic;
using System.Threading.Tasks;
using AzureStorage;
using Lykke.Service.BcnReports.Core.ReportMetadata;

namespace Lykke.Service.BcnReports.AzureRepositories.ReportMetadata
{

    public class BlockTransactionsReportMetadataRepository: BaseReportMetadataRepository, IBlockTransactionsReportMetadataRepository
    {
        private const int ShardingDivider = 100;
        public BlockTransactionsReportMetadataRepository(INoSQLTableStorage<BaseReportMetadataEntity> storage) : base(storage)
        {
        }

        public override async Task InsertOrReplace(IBaseReportMetadata reportMetadata)
        {
            await Storage.CreateIfNotExistsAsync(BaseReportMetadataEntity.Create(reportMetadata,
                GeneratePartitionKey(reportMetadata.Id)));
        }

        public override string GeneratePartitionKey(string id)
        {
            if (int.TryParse(id, out var num))
            {
                return $"{RoundDown(num)}_{RoundUp(num)}";
            }

            return base.GeneratePartitionKey(id);
        }
        

        private int RoundUp(int toRound)
        {
            return (ShardingDivider - toRound % ShardingDivider) + toRound;
        }

        private int RoundDown(int toRound)
        {
            return toRound - toRound % ShardingDivider;
        }
    }
}
