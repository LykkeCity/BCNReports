using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using AzureStorage.Queue;
using Common;
using Core;
using Core.Queue;

namespace LkeServices.ReportsCommands
{
    public class ReportCommandProducer: IReportCommandProducer
    {
        private readonly IQueueExt _queue;

        public ReportCommandProducer(IQueueExt queue)
        {
            _queue = queue;
        }

        public async Task CreateAddressTransactionsReportCommand(string address, string email)
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
