using System.Threading.Tasks;
using AzureStorage.Queue;
using Common;
using Lykke.Service.BcnReports.Core.Queue;

namespace Lykke.Service.BcnReports.AzureRepositories.ReportsCommands
{
    public class AddressReportCommandProducer: IAddressReportCommandProducer
    {
        private readonly IQueueExt _queue;

        public AddressReportCommandProducer(IQueueExt queue)
        {
            _queue = queue;
        }

        public async Task CreateCommand(string address, string email)
        {
            var msg = new AddressTransactionReportQueueCommand
            {
                Address = address,
                Email = email
            };

            await _queue.PutRawMessageAsync(msg.ToJson());
        }
    }
}
