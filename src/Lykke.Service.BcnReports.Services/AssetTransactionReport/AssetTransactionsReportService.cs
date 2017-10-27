using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Lykke.Service.BcnReports.Core.AddressTransactionReport;
using Lykke.Service.BcnReports.Core.Asset;
using Lykke.Service.BcnReports.Core.AssetTransactionReport;
using Lykke.Service.BcnReports.Core.Settings;
using Lykke.Service.BcnReports.Core.Transaction;
using Lykke.Service.BcnReports.Services.Settings;
using Lykke.Service.BcnReports.Services.Xlsx;

namespace Lykke.Service.BcnReports.Services.AssetTransactionReport
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
