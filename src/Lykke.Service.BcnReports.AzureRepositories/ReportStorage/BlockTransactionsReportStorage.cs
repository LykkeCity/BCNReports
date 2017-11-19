using AzureRepositories.ReportStorage;
using AzureStorage;
using Common.Log;
using Lykke.Service.BcnReports.Core.ReportStorage;

namespace Lykke.Service.BcnReports.AzureRepositories.ReportStorage
{
    public class BlockTransactionsReportStorage : BaseReportStorage, IBlockTransactionsReportStorage
    {
        private const string Container = "blocktransactionreports";

        private const int ShardingDivider = 1000;

        public BlockTransactionsReportStorage(ILog log, IBlobStorage blobStorage) : base(Container, log, blobStorage)
        {
        }

        public override string GeneratePartition(string id)
        {
            if (int.TryParse(id, out var num))
            {
                return $"{Container}-{RoundDown(num)}-{RoundUp(num)}";
            }
            return base.GeneratePartition(id);
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
