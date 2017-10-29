using System.Threading.Tasks;

namespace Lykke.Service.BcnReports.Core.Queue
{
    public class BlockTransactionReportQueueCommand
    {
        public string BlockId { get; set; }

        public string Email { get; set; }
    }

    public interface IBlockReportCommandProducer
    {
        Task CreateCommand(string blockId, string email);
    }
}
