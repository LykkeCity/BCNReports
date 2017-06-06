using System.Threading.Tasks;

namespace Core.Queue
{
    public class AddressTransactionReportQueueCommand
    {
        public string Address { get; set; }

        public string Email { get; set; }
    }

    public interface IReportCommandProducer
    {
        Task CreateAddressTransactionsReportCommand(string address, string email);
    }
}
