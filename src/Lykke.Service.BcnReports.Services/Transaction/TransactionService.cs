using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Common.Extensions;
using Common.Log;
using Lykke.Service.BcnReports.AzureRepositories.Helpers;
using Lykke.Service.BcnReports.Core.NinjaClient;
using Lykke.Service.BcnReports.Core.Settings;
using Lykke.Service.BcnReports.Core.Transaction;
using NBitcoin;
using QBitNinja.Client;
using QBitNinja.Client.Models;

namespace Lykke.Service.BcnReports.Services.Transaction
{
    public class TransactionService:ITransactionService
    {
        private readonly SemaphoreSlim _globalSemaphore;
        private readonly INinjaClientFactory _qBitNinjaClient;
        private readonly ILog _log;

        public TransactionService(BcnReportsSettings bcnReportsSettings,
            INinjaClientFactory qBitNinjaClient, 
            ILog log)
        {
            _qBitNinjaClient = qBitNinjaClient;
            _log = log;

            _globalSemaphore = new SemaphoreSlim(bcnReportsSettings.NinjaTransactionsMaxConcurrentRequestCount);
        }

        public async Task<IEnumerable<GetTransactionResponse>> GetTransactions(IEnumerable<uint256> transactionIds)
        {
            var transactionsTasks = new List<Task>();

            var txResps = new ConcurrentBag<GetTransactionResponse>();

            var txIds = transactionIds.ToList();
            foreach (var txId in txIds)
            {
                await _globalSemaphore.WaitAsync();
                var tsk = Retry.Try(() => _qBitNinjaClient.GetClient().GetTransaction(txId).WithTimeout(60 * 1000),
                        component: nameof(GetTransactions),
                        tryCount: 10,
                        logger: _log,
                        secondsToWaitOnFail: 5)
                    .ContinueWith(async p =>
                    {
                        try
                        {
                            await _log.WriteMonitorAsync(nameof(GetTransactions), 
                                nameof(TransactionService),
                                $"Retrieve {txId} done");
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
        }
    }
}
