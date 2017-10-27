using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Common.Log;
using Lykke.Service.BcnReports.AzureRepositories.Helpers;
using Lykke.Service.BcnReports.Core.Address;
using Lykke.Service.BcnReports.Core.AddressTransactionReport;
using Lykke.Service.BcnReports.Core.Asset;
using Lykke.Service.BcnReports.Core.Settings;
using Lykke.Service.BcnReports.Core.Transaction;
using Lykke.Service.BcnReports.Services.Settings;
using Lykke.Service.BcnReports.Services.Xlsx;
using NBitcoin;

namespace Lykke.Service.BcnReports.Services.AddressTransactionReport
{
    public class AddressTransactionReportService : IAddressTransactionReportService
    {
        private readonly ITransactionXlsxRenderer _transactionXlsxRenderer;
        private readonly IAddressService _addressService;
        private readonly IAssetDefinitionService _assetDefinitionService;
        private readonly BcnReportsSettings _bcnReportsSettings;
        private readonly ILog _log;
        private readonly ITransactionService _transactionService;
        
        public AddressTransactionReportService(ITransactionXlsxRenderer transactionXlsxRenderer, 
            IAssetDefinitionService assetDefinitionService, 
            BcnReportsSettings bcnReportsSettings,
            ILog log, 
            IAddressService addressService, 
            ITransactionService transactionService)
        {
            _transactionXlsxRenderer = transactionXlsxRenderer;
            _assetDefinitionService = assetDefinitionService;
            _bcnReportsSettings = bcnReportsSettings;
            _log = log;
            _addressService = addressService;
            _transactionService = transactionService;
        }

        public async Task<Stream> GetTransactionsReport(string addressId)
        {
            var assetDefinitionDictionary = _assetDefinitionService.GetAssetDefinitionsAsync();
            var addressTransactionIds = Retry.Try(() => GetAddressTransactions(addressId), 
                tryCount: 10,
                secondsToWaitOnFail: 5,
                logger: _log);

            await Task.WhenAll(assetDefinitionDictionary, addressTransactionIds);

            var txResps = await _transactionService.GetTransactions(addressTransactionIds.Result);
            
            var xlsxData = XlsxTransactionsReportData.Create(
                txResps, 
                assetDefinitionDictionary.Result,
                _bcnReportsSettings.UsedNetwork());
            
            return await _transactionXlsxRenderer.RenderTransactionReport(xlsxData);
        }

        private async Task<IEnumerable<uint256>> GetAddressTransactions(string bitcoinAddress)
        {
            return (await _addressService.GetTransactionsForAddress(bitcoinAddress)).Select(
                p => uint256.Parse(p.TransactionId));
        }
    }
}
