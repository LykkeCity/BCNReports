using System;
using System.Threading.Tasks;
using Common;
using Common.Log;
using Core.AddressTramsactionsReport;
using Core.AddressTransactionReport;
using Core.Queue;
using Core.ReportMetadata;
using Lykke.EmailSenderProducer;
using Lykke.EmailSenderProducer.Models;
using Lykke.JobTriggers.Triggers.Attributes;

namespace BackGroundJobs.QueueHandlers
{
    public class AddressFunctionsQueueHandlerFunctions
    {
        private readonly IAddressXlsxService _addressXlsxService;
        private readonly IReportMetadataRepository _reportMetadataRepository;
        private readonly ILog _log;
        private readonly IAddressTransactionsReportRepository _addressTransactionsReportRepository;
        private readonly EmailSenderProducer _emailSenderProducer;

        public AddressFunctionsQueueHandlerFunctions(IAddressXlsxService addressXlsxService, 
            ILog log, 
            EmailSenderProducer emailSenderProducer, 
            IReportMetadataRepository reportMetadataRepository, 
            IAddressTransactionsReportRepository addressTransactionsReportRepository)
        {
            _addressXlsxService = addressXlsxService;
            _log = log;
            _emailSenderProducer = emailSenderProducer;
            _reportMetadataRepository = reportMetadataRepository;
            _addressTransactionsReportRepository = addressTransactionsReportRepository;
        }

        [QueueTrigger(QueueNames.AddressTransactionsReport, notify:true)]
        public async Task CreateReport(AddressTransactionReportQueueCommand command)
        {
            try
            {
                await _reportMetadataRepository.SetProcessing(command.Address);
                var reportDate = DateTime.UtcNow;

                var reportData = await _addressXlsxService.GetTransactionsReport(command.Address);


                var saveResult = await _addressTransactionsReportRepository.Save(command.Address, reportData);
                

                var emailMes = new EmailMessage
                {
                    Subject = $"Report for {command.Address} at {reportDate:f}",
                    Body = $"Report for {command.Address} at {reportDate:f} - {saveResult.Url}",
                };

                if (!string.IsNullOrEmpty(command.Email))
                {
                    await _emailSenderProducer.SendEmailAsync(command.Email, emailMes);
                }

                await _reportMetadataRepository.SetDone(command.Address, saveResult.Url);

                await _log.WriteInfoAsync(nameof(AddressFunctionsQueueHandlerFunctions), 
                    nameof(CreateReport),
                    command.ToJson(), "Report proceeded");
            }
            catch (Exception e)
            {
                await _log.WriteFatalErrorAsync(nameof(AddressFunctionsQueueHandlerFunctions), 
                    nameof(CreateReport),
                    command.ToJson(), e);

                await _reportMetadataRepository.SetError(command.Address, e.ToString());
                throw;
            }

        }
    }
}
