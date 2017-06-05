using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
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

            public static IEnumerable<XlsxTransactionInputOutput> Create(string address, GetTransactionResponse source,
                IDictionary<string, IAssetDefinition> assetDictionary, Network network)
            {
                var index = 0;
                foreach (var inOut in source.SpentCoins)
                {
                    yield return Create(address, inOut, source.Block.BlockId, source.TransactionId, assetDictionary, index, network, CoinType.Input);
                    index++;
                }

                index = 0;
                foreach (var inOut in source.ReceivedCoins)
                {
                    yield return Create(address, inOut, source.Block.BlockId, source.TransactionId, assetDictionary, index, network, CoinType.Output);
                    index++;
                }

            }


            private static XlsxTransactionInputOutput Create(string address, ICoin source, uint256 blockId, uint256 transactionHash,
                IDictionary<string, IAssetDefinition> assetDictionary, int index, Network network, CoinType coinType)
            {

                var result = new XlsxTransactionInputOutput
                {
                    Address = address,
                    BlockHash = blockId.ToString(),
                    TransactionHash = transactionHash.ToString(),
                    Index = index,
                    CoinType = coinType
                };

                var colored = source as ColoredCoin;
                if (colored != null)
                {
                    result.ColouredAssetValue = colored.Amount.Quantity;
                    result.BtcValue = BitcoinUtils.SatoshiToBtc(colored.Bearer.Amount.Satoshi);
                    

                    var assetId = colored.AssetId.GetWif(network).ToString();

                    result.ColouredAssetName = assetDictionary.ContainsKey(assetId) ? assetDictionary[assetId].Name : assetId;
                }

                var uncolored = source as Coin;
                if (uncolored != null)
                {

                    result.BtcValue = BitcoinUtils.SatoshiToBtc(uncolored.Amount.Satoshi);
                }

                return result;
            }
        }


        public static XlsxTransactionsReportData Create(string address, 
            IEnumerable<GetTransactionResponse> transactions,
            IDictionary<string, IAssetDefinition> assetDictionary, Network network)
        {
            return new XlsxTransactionsReportData
            {
                TransactionInputOutputs = transactions.SelectMany(p => XlsxTransactionInputOutput.Create(address, p, assetDictionary, network))
            };
        }

    }

    public class AddressXlsxService : IAddressXlsxService
    {
        private readonly IAddressXlsxRenderer _addressXlsxRenderer;
        private readonly QBitNinjaClient _qBitNinjaClient;
        private readonly IAssetDefinitionService _assetDefinitionService;
        private readonly BaseSettings _baseSettings;
        private static readonly SemaphoreSlim GlobalSemaphore = new SemaphoreSlim(5);

        public AddressXlsxService(IAddressXlsxRenderer addressXlsxRenderer, 
            QBitNinjaClient qBitNinjaClient, IAssetDefinitionService assetDefinitionService, BaseSettings baseSettings)
        {
            _addressXlsxRenderer = addressXlsxRenderer;
            _qBitNinjaClient = qBitNinjaClient;
            _assetDefinitionService = assetDefinitionService;
            _baseSettings = baseSettings;
        }

        public async Task<Stream> GetTransactionsReport(string addressId)
        {
            var assetDefinitionDictionary = _assetDefinitionService.GetAssetDefinitionsAsync();
            var ninjaBalanceResp = _qBitNinjaClient.GetBalance(BitcoinAddressHelper.GetAddress(addressId, _baseSettings.UsedNetwork()));

            await Task.WhenAll(assetDefinitionDictionary, ninjaBalanceResp);

            var transactionsTasks = new List<Task>();

            var txResps = new ConcurrentBag<GetTransactionResponse>();

            foreach (var txId in ninjaBalanceResp.Result.Operations.Select(p => p.TransactionId))
            {
                await GlobalSemaphore.WaitAsync();
                var tsk = _qBitNinjaClient.GetTransaction(txId)
                    .ContinueWith(p =>
                    {
                        try
                        {

                            txResps.Add(p.Result);
                        }
                        finally
                        {
                            GlobalSemaphore.Release(1);
                        }
                    });

                transactionsTasks.Add(tsk);
            }

            await Task.WhenAll(transactionsTasks);

            var xlsxData = XlsxTransactionsReportData.Create(addressId, 
                txResps, 
                assetDefinitionDictionary.Result,
                _baseSettings.UsedNetwork());

            return await _addressXlsxRenderer.RenderTransactionReport(xlsxData);
        }
    }
}
