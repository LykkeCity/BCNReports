using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using AzureRepositories.Helpers;
using Common.Log;
using Core.Address;
using Core.AddressTransactionReport;
using Core.Asset;
using Core.Settings;
using LkeServices.BitcoinHelpers;
using LkeServices.Settings;
using NBitcoin;
using QBitNinja.Client;
using QBitNinja.Client.Models;

namespace LkeServices.AddressTransactionReport
{
    public class XlsxTransactionsReportData : IXlsxTransactionsReportData
    {
        public IEnumerable<IXlsxTransactionInputOutput> TransactionInputOutputs { get; set; }

        public class XlsxTransactionInputOutput : IXlsxTransactionInputOutput
        {
            public string TransactionHash { get; set; }
            public string BlockHash { get; set; }
            public int Index { get; set; }
            public string Address { get; set; }
            public double BtcValue { get; set; }
            public string ColouredAssetName { get; set; }
            public double ColouredAssetValue { get; set; }
            public CoinType CoinType { get; set; }

            public static IEnumerable<XlsxTransactionInputOutput> Create(string sourceAddr, GetTransactionResponse source,
                IDictionary<string, IAssetDefinition> assetDictionary, Network network)
            {
                var index = 0;
                foreach (var inOut in source.SpentCoins)
                {
                    var addr = inOut.TxOut?.ScriptPubKey?.GetDestinationAddress(network)?.ToWif();

                    yield return Create(addr, inOut, source.Block?.BlockId, source.TransactionId, assetDictionary, index, network, CoinType.Input);
                    index++;
                }

                index = 0;
                foreach (var inOut in source.ReceivedCoins)
                {
                    var addr = inOut.TxOut?.ScriptPubKey?.GetDestinationAddress(network)?.ToWif();

                    yield return Create(addr, inOut, source.Block?.BlockId, source.TransactionId, assetDictionary, index, network, CoinType.Output);
                    index++;
                }

                yield return CreateFees(sourceAddr, source.Fees, source.Block?.BlockId, source.TransactionId, index);
            }


            private static XlsxTransactionInputOutput Create(string sourceAddr, ICoin source, uint256 blockId, uint256 transactionHash,
                IDictionary<string, IAssetDefinition> assetDictionary, int index, Network network, CoinType coinType)
            {
                var result = new XlsxTransactionInputOutput
                {
                    Address = sourceAddr,
                    BlockHash = blockId?.ToString(),
                    TransactionHash = transactionHash.ToString(),
                    Index = index,
                    CoinType = coinType,
                };

                var colored = source as ColoredCoin;
                if (colored != null)
                {
                    var assetId = colored.AssetId.GetWif(network).ToString();

                    var asset = assetDictionary.ContainsKey(assetId) ? assetDictionary[assetId] : null;
                    var divisibility = asset?.Divisibility ?? 0;

                    result.ColouredAssetValue = BitcoinUtils.CalculateColoredAssetQuantity(colored.Amount.Quantity, divisibility);
                    result.BtcValue = BitcoinUtils.SatoshiToBtc(colored.Bearer.Amount.Satoshi);

                    result.ColouredAssetName = asset != null ? asset.Name : assetId;
                }

                var uncolored = source as Coin;
                if (uncolored != null)
                {

                    result.BtcValue = BitcoinUtils.SatoshiToBtc(uncolored.Amount.Satoshi);
                }

                return result;
            }

            private static XlsxTransactionInputOutput CreateFees(string address, Money fees, uint256 blockId, uint256 transactionHash,int index )
            {
                return new XlsxTransactionInputOutput
                {
                    Address = address,
                    BlockHash = blockId?.ToString(),
                    TransactionHash = transactionHash.ToString(),
                    Index = index,
                    CoinType = CoinType.Fees,
                    BtcValue = BitcoinUtils.SatoshiToBtc(fees.Satoshi)
                };
            }
        }


        public static XlsxTransactionsReportData Create(string address, 
            IEnumerable<GetTransactionResponse> transactions,
            IDictionary<string, IAssetDefinition> assetDictionary, Network network)
        {
            return new XlsxTransactionsReportData
            {
                TransactionInputOutputs = transactions.SelectMany(p => XlsxTransactionInputOutput.Create(address, p, assetDictionary, network)).ToList()
            };
        }

    }

    public class AddressXlsxService : IAddressXlsxService
    {
        private readonly IAddressXlsxRenderer _addressXlsxRenderer;
        private readonly IAddressService _addressService;
        private readonly QBitNinjaClient _qBitNinjaClient;
        private readonly IAssetDefinitionService _assetDefinitionService;
        private readonly BaseSettings _baseSettings;
        private readonly SemaphoreSlim _globalSemaphore;
        private readonly ILog _log;

        public AddressXlsxService(IAddressXlsxRenderer addressXlsxRenderer, 
            QBitNinjaClient qBitNinjaClient, 
            IAssetDefinitionService assetDefinitionService, 
            BaseSettings baseSettings,
            ILog log, 
            IAddressService addressService)
        {
            _addressXlsxRenderer = addressXlsxRenderer;
            _qBitNinjaClient = qBitNinjaClient;
            _assetDefinitionService = assetDefinitionService;
            _baseSettings = baseSettings;
            _log = log;
            _addressService = addressService;

            _globalSemaphore = new SemaphoreSlim(baseSettings.NinjaTransactionsMaxConcurrentRequestCount);
        }

        public async Task<Stream> GetTransactionsReport(string addressId)
        {
            var assetDefinitionDictionary = _assetDefinitionService.GetAssetDefinitionsAsync();
            var addressTransactionIds = GetAddressTransactions(addressId);

            await Task.WhenAll(assetDefinitionDictionary, addressTransactionIds);

            var transactionsTasks = new List<Task>();

            var txResps = new ConcurrentBag<GetTransactionResponse>();

            var txIds = addressTransactionIds.Result.ToList();
            foreach (var txId in txIds)
            {
                await _globalSemaphore.WaitAsync();
                var tsk = Retry.Try( () => _qBitNinjaClient.GetTransaction(txId), 
                    exceptionFilter:p => true, 
                    tryCount: 10, 
                    logger: _log, 
                    secondsToWaitOnFail:5)
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

            var xlsxData = XlsxTransactionsReportData.Create(addressId, 
                txResps.OrderBy(p => txIds.IndexOf(p.TransactionId)), 
                assetDefinitionDictionary.Result,
                _baseSettings.UsedNetwork());
            
            return await _addressXlsxRenderer.RenderTransactionReport(xlsxData);
        }

        private async Task<IEnumerable<uint256>> GetAddressTransactions(string bitcoinAddress)
        {
            return (await _addressService.GetTransactionsForAddress(bitcoinAddress)).Select(
                p => uint256.Parse(p.TransactionId));
        }
    }
}
