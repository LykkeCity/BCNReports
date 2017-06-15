using System.IO;
using System.Threading.Tasks;

namespace Core.AddressTransactionReport
{
    public interface IAddressTransactionReportService
    {
        Task<Stream> GetTransactionsReport(string addressId);
    }
}
