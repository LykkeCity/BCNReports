using System.IO;
using System.Threading.Tasks;

namespace Core.AddressTransactionReport
{
    public interface IAddressXlsxService
    {
        Task<Stream> GetTransactionsReport(string addressId);
    }
}
