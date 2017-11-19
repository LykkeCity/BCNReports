using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Lykke.Service.BcnReports.Core.Queue
{
    public class BlockTransactionRangeReportQueueCommand
    {
        public int From { get; set; }

        public int To { get; set; }

        public int Batch { get; set; }
    }
    public interface IBlockRangeReportCommandProducer
    {
        Task CreateRangeReport(int fromBlock, int toBlock, int batch);
    }
}
