using System.Threading.Tasks;

namespace Lykke.Service.BcnReports.Core.Queue
{
    public class AddressTransactionReportQueueCommand
    {
        public string Address { get; set; }

        public string Email { get; set; }
    }

    public interface IAddressReportCommandProducer
    {
        Task CreateCommand(string address, string email);
    }
}
