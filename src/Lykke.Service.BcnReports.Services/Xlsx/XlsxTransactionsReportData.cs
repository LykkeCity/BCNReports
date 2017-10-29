using System;
using System.Collections.Generic;
using System.Linq;
using LkeServices.BitcoinHelpers;
using Lykke.Service.BcnReports.Core.AddressTransactionReport;
using Lykke.Service.BcnReports.Core.Asset;
using Lykke.Service.BcnReports.Core.Xlsx;
using NBitcoin;
using QBitNinja.Client.Models;

namespace Lykke.Service.BcnReports.Services.Xlsx
{
    public class XlsxTransactionsReportData : IXlsxTransactionsReportData
    {
        public IEnumerable<IXlsxTransactionInputOutput> TransactionInputOutputs { get; set; }

        public class XlsxTransactionInputOutput : IXlsxTransactionInputOutput
        {
            public string TransactionHash { get; set; }
            public string BlockHash { get; set; }
            public DateTime? BlockDate { get; set; }
            public int Index { get; set; }
            public string Address { get; set; }
            public string ColoredAddress { get; set; }
            public double BtcValue { get; set; }
            public string ColouredAssetName { get; set; }
            public double ColouredAssetValue { get; set; }
            public CoinType CoinType { get; set; }

            public static IEnumerable<XlsxTransactionInputOutput> Create( GetTransactionResponse source,
                IDictionary<string, IAssetDefinition> assetDictionary, Network network)
            {
                var index = 0;
                foreach (var inOut in source.SpentCoins)
                {
                    var addr = inOut.TxOut?.ScriptPubKey?.GetDestinationAddress(network);
                    
                    yield return Create(addr,
                        inOut, 
                        source.Block, 
                        source.TransactionId, 
                        assetDictionary, 
                        index, 
                        network, 
                        CoinType.Input);

                    index++;
                }

                index = 0;
                foreach (var inOut in source.ReceivedCoins)
                {
                    var addr = inOut.TxOut?.ScriptPubKey?.GetDestinationAddress(network);

                    yield return Create(addr, 
                        inOut, 
                        source.Block, 
                        source.TransactionId, 
                        assetDictionary, 
                        index, 
                        network, 
                        CoinType.Output);

                    index++;
                }

                yield return CreateFees(source.Fees, source.Block, source.TransactionId, index);
            }


            private static XlsxTransactionInputOutput Create(BitcoinAddress address,
                ICoin source, 
                BlockInformation block,
                uint256 transactionHash,
                IDictionary<string, IAssetDefinition> assetDictionary, 
                int index, 
                Network network, 
                CoinType coinType)
            {
                string coloredAddress = null;
                try
                {
                    coloredAddress = address?.ToColoredAddress()?.ToWif();
                }
                catch 
                {
                }
                var result = new XlsxTransactionInputOutput
                {
                    Address = address?.ToString(),
                    ColoredAddress = coloredAddress,
                    TransactionHash = transactionHash.ToString(),
                    Index = index,
                    CoinType = coinType,
                };

                if (block != null)
                {
                    result.BlockDate = block.BlockTime.UtcDateTime;
                    result.BlockHash = block.BlockId.ToString();
                }
                if (source is ColoredCoin colored)
                {
                    var assetId = colored.AssetId.GetWif(network).ToString();

                    var asset = assetDictionary.ContainsKey(assetId) ? assetDictionary[assetId] : null;
                    var divisibility = asset?.Divisibility ?? 0;

                    result.ColouredAssetValue = BitcoinUtils.CalculateColoredAssetQuantity(colored.Amount.Quantity, divisibility);
                    result.BtcValue = BitcoinUtils.SatoshiToBtc(colored.Bearer.Amount.Satoshi);

                    result.ColouredAssetName = asset != null ? asset.Name : assetId;
                }

                if (source is Coin uncolored)
                {

                    result.BtcValue = BitcoinUtils.SatoshiToBtc(uncolored.Amount.Satoshi);
                }

                return result;
            }

            private static XlsxTransactionInputOutput CreateFees(Money fees, BlockInformation block, uint256 transactionHash, int index)
            {
                var result =  new XlsxTransactionInputOutput
                {
                    TransactionHash = transactionHash.ToString(),
                    Index = index,
                    CoinType = CoinType.Fees,
                    BtcValue = BitcoinUtils.SatoshiToBtc(fees.Satoshi)
                };

                if (block != null)
                {
                    result.BlockDate = block.BlockTime.UtcDateTime;
                    result.BlockHash = block.BlockId.ToString();
                }

                return result;
            }
        }


        public static XlsxTransactionsReportData Create(
            IEnumerable<GetTransactionResponse> transactions,
            IDictionary<string, IAssetDefinition> assetDictionary, 
            Network network)
        {
            return new XlsxTransactionsReportData
            {
                TransactionInputOutputs = transactions.SelectMany(p => XlsxTransactionInputOutput.Create(p, assetDictionary, network)).ToList()
            };
        }

    }
}
