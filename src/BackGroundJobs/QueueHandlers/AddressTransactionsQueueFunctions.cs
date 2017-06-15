using System;
using System.Threading.Tasks;
using Common;
using Common.Log;
using Core.AddressTransactionReport;
using Core.Queue;
using Core.ReportMetadata;
using Core.ReportStorage;
using Lykke.EmailSenderProducer;
using Lykke.EmailSenderProducer.Models;
using Lykke.JobTriggers.Triggers.Attributes;

namespace BackGroundJobs.QueueHandlers
{
    public class AddressTransactionsQueueFunctions
    {
        private readonly IAddressTransactionReportService _addressTransactionReportService;
        private readonly IAddressTransactionsReportMetadataRepository _addressTransactionsReportMetadataRepository;
        private readonly ILog _log;
        private readonly IAddressTransactionsReportStorage _addressTransactionsReportStorage;
        private readonly EmailSenderProducer _emailSenderProducer;

        public AddressTransactionsQueueFunctions(
            IAddressTransactionReportService addressTransactionReportService, 
            ILog log, 
            EmailSenderProducer emailSenderProducer,
            IAddressTransactionsReportMetadataRepository addressTransactionsReportMetadataRepository,
            IAddressTransactionsReportStorage addressTransactionsReportStorage)
        {
            _addressTransactionReportService = addressTransactionReportService;
            _log = log;
            _emailSenderProducer = emailSenderProducer;
            _addressTransactionsReportMetadataRepository = addressTransactionsReportMetadataRepository;
            _addressTransactionsReportStorage = addressTransactionsReportStorage;
        }

        [QueueTrigger(QueueNames.AddressTransactionsReport, notify:true)]
        public async Task CreateReport(AddressTransactionReportQueueCommand command)
        {
            try
            {
                await _addressTransactionsReportMetadataRepository.SetProcessing(command.Address);
                var reportDate = DateTime.UtcNow;

                var reportData = await _addressTransactionReportService.GetTransactionsReport(command.Address);


                var saveResult = await _addressTransactionsReportStorage.Save(command.Address, reportData);
                

                var emailMes = new EmailMessage
                {
                    Subject = $"Report for {command.Address} at {reportDate:f}",
                    Body = $"Report for {command.Address} at {reportDate:f} - {saveResult.Url}",
                };

                if (!string.IsNullOrEmpty(command.Email))
                {
                    await _emailSenderProducer.SendEmailAsync(command.Email, emailMes);
                }

                await _addressTransactionsReportMetadataRepository.SetDone(command.Address, saveResult.Url);

                await _log.WriteInfoAsync(nameof(AddressTransactionsQueueFunctions), 
                    nameof(CreateReport),
                    command.ToJson(), "Report proceeded");
            }
            catch (Exception e)
            {
                await _log.WriteFatalErrorAsync(nameof(AddressTransactionsQueueFunctions), 
                    nameof(CreateReport),
                    command.ToJson(), e);

                await _addressTransactionsReportMetadataRepository.SetError(command.Address, e.ToString());
                throw;
            }

        }
    }
}
