using System.Collections.Generic;
using System.Threading.Tasks;
using NBitcoin;
using QBitNinja.Client.Models;

namespace Lykke.Service.BcnReports.Core.Transaction
{
    public interface ITransactionService
    {
        Task<IEnumerable<GetTransactionResponse>> GetTransactions(IEnumerable<uint256> transactionIds);
        Task<IEnumerable<GetTransactionResponse>> GetTransactions(IEnumerable<string> transactionIds);
    }
}
