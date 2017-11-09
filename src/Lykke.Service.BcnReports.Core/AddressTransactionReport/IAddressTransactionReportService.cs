using System.IO;
using System.Threading.Tasks;

namespace Lykke.Service.BcnReports.Core.AddressTransactionReport
{
    public interface IAddressTransactionReportService
    {
        Task<Stream> GetTransactionsReport(string addressId);
    }
}
