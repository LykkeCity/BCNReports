using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Lykke.Service.BcnReports.Core.Asset;
using Lykke.Service.BcnReports.Core.BlockTransactionsReport;
using Lykke.Service.BcnReports.Core.Transaction;
using Lykke.Service.BcnReports.Core.Xlsx;
using Lykke.Service.BcnReports.Services.Xlsx;
using NBitcoin;
using QBitNinja.Client;
using QBitNinja.Client.Models;

namespace Lykke.Service.BcnReports.Services.BlockTransactionsReport
{
    public class BlockTransactionsReportService: IBlockTransactionsReportService
    {
        private readonly QBitNinjaClient _bitNinjaClient;
        private readonly IAssetDefinitionService _assetDefinitionService;
        private readonly ITransactionService _transactionService;
        private readonly ITransactionXlsxRenderer _transactionXlsxRenderer;
        private readonly Network _network;

        public BlockTransactionsReportService(QBitNinjaClient bitNinjaClient, 
            IAssetDefinitionService assetDefinitionService, 
            ITransactionService transactionService,
            ITransactionXlsxRenderer transactionXlsxRenderer, 
            Network network)
        {
            _bitNinjaClient = bitNinjaClient;
            _assetDefinitionService = assetDefinitionService;
            _transactionService = transactionService;
            _transactionXlsxRenderer = transactionXlsxRenderer;
            _network = network;
        }

        public async Task<Stream> GetTransactionsReport(string blockId)
        {
            var getBlock = _bitNinjaClient.GetBlock(BlockFeature.Parse(blockId));
            var getAssetDefDictionary = _assetDefinitionService.GetAssetDefinitionsAsync();

            await Task.WhenAll(getBlock, getAssetDefDictionary);

            if (getBlock.Result == null)
            {
                throw new Exception($"Block {blockId} not found");
            }

            var transactionIds = getBlock.Result.Block.Transactions.Select(p => p.GetHash());

            var txResps = await _transactionService.GetTransactions(transactionIds);

            var xlsxData = XlsxTransactionsReportData.Create(
                txResps,
                getAssetDefDictionary.Result,
                _network);

            return await _transactionXlsxRenderer.RenderTransactionReport(xlsxData);
        }
    }
}
