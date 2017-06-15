using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Common.Log;
using Core.AddressTransactionReport;
using Core.Asset;
using Core.AssetTransactionReport;
using Core.Settings;
using Core.Transaction;
using LkeServices.Settings;
using LkeServices.Xlsx;

namespace LkeServices.AssetTransactionReport
{
    public class AssetTransactionsReportService:IAssetTransactionsReportService
    {
        private readonly ITransactionXlsxRenderer _transactionXlsxRenderer;
        private readonly ITransactionService _transactionService;
        private readonly IAssetDefinitionService _assetDefinitionService;
        private readonly IAssetTransactionsService _assetTransactionsService;
        private readonly BaseSettings _baseSettings;

        public AssetTransactionsReportService(ITransactionXlsxRenderer transactionXlsxRenderer, 
            ITransactionService transactionService, 
            IAssetDefinitionService assetDefinitionService, 
            IAssetTransactionsService assetTransactionsService, 
            BaseSettings baseSettings)
        {
            _transactionXlsxRenderer = transactionXlsxRenderer;
            _transactionService = transactionService;
            _assetDefinitionService = assetDefinitionService;
            _assetTransactionsService = assetTransactionsService;
            _baseSettings = baseSettings;
        }

        public async Task<Stream> GetTransactionsReport(string assetId)
        {
            var transactionIds = _assetTransactionsService.GetTransactionsForAsset(assetId);
            var assetDictionary = _assetDefinitionService.GetAssetDefinitionsAsync();

            await Task.WhenAll(transactionIds, assetDictionary);

            var txResps = await _transactionService.GetTransactions(transactionIds.Result.Select(p => p.TransactionId));

            var xlsxData = XlsxTransactionsReportData.Create(
                txResps,
                assetDictionary.Result,
                _baseSettings.UsedNetwork());

            return await _transactionXlsxRenderer.RenderTransactionReport(xlsxData);
        }
    }
}
