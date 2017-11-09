using System.Collections.Generic;
using System.Threading.Tasks;

namespace Lykke.Service.BcnReports.Core.Queue
{
    public class BlockTransactionReportQueueCommand
    {
        public string[] Blocks { get; set; }

        public string Email { get; set; }
    }

    public interface IBlockReportCommandProducer
    {
        Task CreateCommand(IEnumerable<string> blocks, string email);
    }
}
