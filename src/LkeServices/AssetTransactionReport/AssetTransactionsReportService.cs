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
        private readonly BcnReportsSettings _bcnReportsSettings;

        public AssetTransactionsReportService(ITransactionXlsxRenderer transactionXlsxRenderer, 
            ITransactionService transactionService, 
            IAssetDefinitionService assetDefinitionService, 
            IAssetTransactionsService assetTransactionsService, 
            BcnReportsSettings bcnReportsSettings)
        {
            _transactionXlsxRenderer = transactionXlsxRenderer;
            _transactionService = transactionService;
            _assetDefinitionService = assetDefinitionService;
            _assetTransactionsService = assetTransactionsService;
            _bcnReportsSettings = bcnReportsSettings;
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
                _bcnReportsSettings.UsedNetwork());

            return await _transactionXlsxRenderer.RenderTransactionReport(xlsxData);
        }
    }
}
