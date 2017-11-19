using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Common;
using Common.Log;
using Flurl;
using Flurl.Http;
using LkeServices.BitcoinHelpers;
using Lykke.Service.BcnReports.AzureRepositories.Helpers;
using Lykke.Service.BcnReports.Core.Asset;
using Lykke.Service.BcnReports.Core.Block;
using Lykke.Service.BcnReports.Core.BlockTransactionsReport;
using Lykke.Service.BcnReports.Core.Console;
using Lykke.Service.BcnReports.Core.Settings;
using Lykke.Service.BcnReports.Core.Transaction;
using Lykke.Service.BcnReports.Core.Xlsx;
using Lykke.Service.BcnReports.Services.Address;
using Lykke.Service.BcnReports.Services.Xlsx;
using NBitcoin;
using QBitNinja.Client;
using QBitNinja.Client.Models;

namespace Lykke.Service.BcnReports.Services.BlockTransactionsReport
{
    public class BlockXlsxTransactionsReportData : IXlsxTransactionsReportData
    {
        public IEnumerable<IXlsxTransactionInputOutput> TransactionInputOutputs { get; set; }

        public static async Task<BlockXlsxTransactionsReportData> CreateAsync(TransactionListContract transactionList, 
            DateTime? blockDate,
            IDictionary<string, IAssetDefinition> assetDictionary,
            Network network)
        {
            var txs = await transactionList.Transactions.SelectAsync(p =>Task.Run(()=> BlockXlsxTransactionInputOutput.Create(p, blockDate, assetDictionary, network).ToList()));
            return new BlockXlsxTransactionsReportData
            {
                TransactionInputOutputs = txs.SelectMany(p=>p)
            };
        }
    }

    public class BlockXlsxTransactionInputOutput : IXlsxTransactionInputOutput
    {
        public string TransactionHash { get; set; }
        public string BlockHash { get; set; }
        public DateTime? BlockDate { get; set; }
        public int? Index { get; set; }
        public string Address { get; set; }
        public string ColoredAddress { get; set; }
        public double BtcValue { get; set; }
        public string ColouredAssetName { get; set; }
        public double ColouredAssetValue { get; set; }
        public CoinType CoinType { get; set; }

        public static IEnumerable<BlockXlsxTransactionInputOutput> Create(TransactionListItemContract transaction,
            DateTime? blockDate,
            IDictionary<string, IAssetDefinition> assetDictionary,
            Network network)
        {
            foreach (var inOut in transaction.Spent)
            {
                yield return Create(inOut, blockDate, transaction.BlockId, assetDictionary, network, transaction.TxId,
                    CoinType.Input);
            }

            foreach (var inOut in transaction.Received){
                yield return Create(inOut, blockDate, transaction.BlockId, assetDictionary, network, transaction.TxId,
                    CoinType.Output);
            }

            yield return CreateFees(transaction.Fees ?? 0, blockDate, transaction.BlockId, transaction.TxId);
        }

        private static BlockXlsxTransactionInputOutput Create(InOutContract inOut,
            DateTime? blockDate,
            string blockHash,
            IDictionary<string, IAssetDefinition> assetDictionary,
            Network network,
            string transactionHash,
            CoinType coinType)
        {
            string coloredAddress = null;
            //try
            //{
            //    coloredAddress = BitcoinAddress.Create(inOut.Address, network).ToColoredAddress().ToWif();
            //}
            //catch
            //{
            //}

            var result = new BlockXlsxTransactionInputOutput
            {
                Address = inOut.Address,
                ColoredAddress = coloredAddress,
                TransactionHash = transactionHash,
                Index = inOut.Index,
                CoinType = coinType,
                BlockDate = blockDate,
                BlockHash = blockHash,
                BtcValue = BitcoinUtils.SatoshiToBtc(inOut.Value)
            };


            if (inOut.AssetId != null)
            {

                var asset = assetDictionary.ContainsKey(inOut.AssetId) ? assetDictionary[inOut.AssetId] : null;
                var divisibility = asset?.Divisibility ?? 0;

                result.ColouredAssetValue = BitcoinUtils.CalculateColoredAssetQuantity(inOut.Quantity ?? 0, divisibility);

                result.ColouredAssetName = asset != null ? asset.Name : inOut.AssetId;
            }

            return result;
        }

        private static BlockXlsxTransactionInputOutput CreateFees(double fees, DateTime? blockDate,
            string blockHash, string transactionHash)
        {
            var result = new BlockXlsxTransactionInputOutput
            {
                TransactionHash = transactionHash,
                CoinType = CoinType.Fees,
                BtcValue = BitcoinUtils.SatoshiToBtc(fees),
                BlockDate = blockDate,
                BlockHash = blockHash
            };
            

            return result;
        }
    }

    public class BlockTransactionsReportService: IBlockTransactionsReportService
    {
        private readonly IBlockService _blockService;
        private readonly IAssetDefinitionService _assetDefinitionService;
        private readonly ITransactionService _transactionService;
        private readonly ITransactionXlsxRenderer _transactionXlsxRenderer;
        private readonly Network _network;
        private readonly BcnReportsSettings _bcnReportsSettings;
        private readonly ILog _log;
        private readonly IConsole _console;
        private readonly SemaphoreSlim _blocktransactionApiSemapthore = new SemaphoreSlim(10);

        public BlockTransactionsReportService(
            IAssetDefinitionService assetDefinitionService, 
            ITransactionService transactionService,
            ITransactionXlsxRenderer transactionXlsxRenderer, 
            Network network, IBlockService blockService, 
            BcnReportsSettings bcnReportsSettings, 
            ILog log, IConsole console)
        { 
            _assetDefinitionService = assetDefinitionService;
            _transactionService = transactionService;
            _transactionXlsxRenderer = transactionXlsxRenderer;
            _network = network;
            _blockService = blockService;
            _bcnReportsSettings = bcnReportsSettings;
            _log = log;
            _console = console;
        }

        public async Task<Stream> GetTransactionsReport(string blockId)
        {
            var getBlockTransactions = GetBlockTransactions(blockId);

            var getBlockHeder = GetBlockHeader(blockId);

            var getAssetDefDictionary = _assetDefinitionService.GetAssetDefinitionsAsync();

            await Task.WhenAll(getBlockTransactions, getAssetDefDictionary, getBlockHeder);

            DateTime? blockDate = null;
            if (getBlockHeder.Result?.AdditionalInformation != null)
            {
                blockDate = getBlockHeder.Result?.AdditionalInformation.BlockTime.UtcDateTime;
            }
            var t = new Stopwatch();
            t.Start();
            var xlsxData = await BlockXlsxTransactionsReportData.CreateAsync(getBlockTransactions.Result, blockDate, getAssetDefDictionary.Result,
                    _network);
            t.Stop();
            _console.WriteLine(t.Elapsed.Seconds.ToString());
            return await _transactionXlsxRenderer.RenderTransactionReport(xlsxData);
        }

        private async Task<GetBlockResponse> GetBlockHeader(string blockId)
        {
            _console.WriteConsoleLog(nameof(IBlockTransactionsReportService), nameof(GetBlockHeader), blockId, "Started");
            var result = await Retry.Try(() => _blockService.GetBlock(BlockFeature.Parse(blockId), headerOnly: true),
                nameof(GetBlockHeader), 
                tryCount: 5, 
                secondsToWaitOnFail: 3);

            _console.WriteConsoleLog(nameof(IBlockTransactionsReportService), nameof(GetBlockHeader), blockId, "Done");

            return result;
        }

        private async Task<TransactionListContract> GetBlockTransactions(string blockId)
        {
            _console.WriteConsoleLog(nameof(IBlockTransactionsReportService), nameof(GetBlockTransactions), blockId, "Started");

            var result = await Retry.Try(async () =>
                {
                    try
                    {
                        await _blocktransactionApiSemapthore.WaitAsync();

                        return await _bcnReportsSettings.BlockTransactionsServiceUrl
                            .AppendPathSegment($"/blocks/{blockId}/transactions")
                            .GetJsonAsync<TransactionListContract>();
                    }
                    finally
                    {
                        _blocktransactionApiSemapthore.Release();
                    }

                },
                nameof(GetBlockTransactions), 
                tryCount: 5,
                secondsToWaitOnFail: 3);

            _console.WriteConsoleLog(nameof(IBlockTransactionsReportService), nameof(GetBlockTransactions), blockId, "Done");

            return result;
        }
    }
}
