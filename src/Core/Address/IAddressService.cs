using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Core.Address
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
