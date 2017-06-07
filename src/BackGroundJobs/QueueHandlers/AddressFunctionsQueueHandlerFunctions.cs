using System;
using System.Threading.Tasks;
using Common;
using Common.Log;
using Core.AddressTransactionReport;
using Core.Queue;
using Core.ServiceMonitoring;
using Lykke.EmailSenderProducer;
using Lykke.EmailSenderProducer.Models;
using Lykke.JobTriggers.Triggers.Attributes;
using OfficeOpenXml.FormulaParsing.Excel.Functions.DateTime;

namespace BackGroundJobs.QueueHandlers
{
    public class AddressFunctionsQueueHandlerFunctions
    {
        private readonly IAddressXlsxService _addressXlsxService;
        private readonly ILog _log;
        private readonly EmailSenderProducer _emailSenderProducer;

        public AddressFunctionsQueueHandlerFunctions(IAddressXlsxService addressXlsxService, 
            ILog log, 
            EmailSenderProducer emailSenderProducer)
        {
            _addressXlsxService = addressXlsxService;
            _log = log;
            _emailSenderProducer = emailSenderProducer;
        }

        [QueueTrigger(QueueNames.AddressTransactionsReport, notify:true)]
        public async Task CreateReport(AddressTransactionReportQueueCommand command)
        {
            try
            {
                
                var reportDate = DateTime.UtcNow;

                var reportData = await _addressXlsxService.GetTransactionsReport(command.Address);
                reportData.Position = 0;

                var emailMes = new EmailMessage
                {
                    Subject = $"Report for {command.Address} at {reportDate:f}",
                    Body = $"Report for {command.Address} at {reportDate:f}",
                    Attachments = new[]
                    {
                        new EmailAttachment
                        {
                            ContentType ="application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                            FileName = $"{command.Address}.xlsx",
                            Stream = reportData
                        }
                    }
                };

                await _emailSenderProducer.SendEmailAsync(command.Email, emailMes);
                await _log.WriteInfoAsync(nameof(AddressFunctionsQueueHandlerFunctions), 
                    nameof(CreateReport),
                    command.ToJson(), "Report proceeded");
            }
            catch (Exception e)
            {
                await _log.WriteFatalErrorAsync(nameof(AddressFunctionsQueueHandlerFunctions), 
                    nameof(CreateReport),
                    command.ToJson(), e);
                throw;
            }

        }
    }
}
