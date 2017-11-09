using System.Collections.Generic;
using System.Threading.Tasks;

namespace Lykke.Service.BcnReports.Core.Address
{
    public interface IAddressTransaction
    {
        string TransactionId { get; }
    }
    public interface IAddressService
    {
        Task<IEnumerable<IAddressTransaction>> GetTransactionsForAddress(string address);
    }
}
