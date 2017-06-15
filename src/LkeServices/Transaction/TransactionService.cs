using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using AzureRepositories.Helpers;
using Common.Log;
using Core;
using Core.Settings;
using Core.Transaction;
using NBitcoin;
using QBitNinja.Client;
using QBitNinja.Client.Models;

namespace LkeServices.Transaction
{
    public class TransactionService:ITransactionService
    {
        private readonly SemaphoreSlim _globalSemaphore;
        private readonly QBitNinjaClient _qBitNinjaClient;
        private readonly ILog _log;

        public TransactionService(BaseSettings baseSettings, 
            QBitNinjaClient qBitNinjaClient, 
            ILog log)
        {
            _qBitNinjaClient = qBitNinjaClient;
            _log = log;

            _globalSemaphore = new SemaphoreSlim(baseSettings.NinjaTransactionsMaxConcurrentRequestCount);
        }

        public async Task<IEnumerable<GetTransactionResponse>> GetTransactions(IEnumerable<uint256> transactionIds)
        {
            var transactionsTasks = new List<Task>();

            var txResps = new ConcurrentBag<GetTransactionResponse>();

            var txIds = transactionIds.ToList();
            foreach (var txId in txIds)
            {
                await _globalSemaphore.WaitAsync();
                var tsk = Retry.Try(() => _qBitNinjaClient.GetTransaction(txId),
                        tryCount: 10,
                        logger: _log,
                        secondsToWaitOnFail: 5)
                    .ContinueWith(p =>
                    {
                        try
                        {

                            txResps.Add(p.Result);
                        }
                        finally
                        {
                            _globalSemaphore.Release(1);
                        }
                    });

                transactionsTasks.Add(tsk);
            }

            await Task.WhenAll(transactionsTasks);

            return txResps.OrderBy(p => txIds.IndexOf(p.TransactionId));
        }

        public Task<IEnumerable<GetTransactionResponse>> GetTransactions(IEnumerable<string> transactionIds)
        {
            return GetTransactions(transactionIds.Select(uint256.Parse));
            throw new NotImplementedException();
        }
    }
}
