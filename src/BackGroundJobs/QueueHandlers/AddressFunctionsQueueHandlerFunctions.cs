using System.Threading.Tasks;
using Core.AddressTransactionReport;
using Core.Queue;
using Core.ServiceMonitoring;
using Lykke.JobTriggers.Triggers.Attributes;

namespace BackGroundJobs.QueueHandlers
{
    public class AddressFunctionsQueueHandlerFunctions
    {
        private readonly IAddressXlsxService _addressXlsxService;

        public AddressFunctionsQueueHandlerFunctions(IAddressXlsxService addressXlsxService)
        {
            _addressXlsxService = addressXlsxService;
        }

        [QueueTrigger(QueueNames.AddressTransactionsReport)]
        public async Task CreateReport(AddressTransactionReportQueueCommand command)
        {
            var reportData = await _addressXlsxService.GetTransactionsReport(command.Address);
        }
    }
}
