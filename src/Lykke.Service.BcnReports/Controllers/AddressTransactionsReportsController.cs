using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LkeServices.BitcoinHelpers;
using Lykke.Service.BcnReports.Core.Queue;
using Lykke.Service.BcnReports.Core.ReportMetadata;
using Lykke.Service.BcnReports.Core.Settings;
using Lykke.Service.BcnReports.Models;
using Lykke.Service.BcnReports.Services.Settings;
using Microsoft.AspNetCore.Mvc;
using Web.Helpers;

namespace Lykke.Service.BcnReports.Controllers
{
    [Route("api/[controller]")]
    public class AddressTransactionsReportsController:Controller
    {
        private readonly IAddressReportCommandProducer _commandProducer;
        private readonly IAddressTransactionsReportMetadataRepository _addressTransactionsReportMetadataRepository;
        private readonly BcnReportsSettings _bcnReportsSettings;

        public AddressTransactionsReportsController(BcnReportsSettings bcnReportsSettings, 
            IAddressReportCommandProducer commandProducer, 
            IAddressTransactionsReportMetadataRepository addressTransactionsReportMetadataRepository)
        {
            _bcnReportsSettings = bcnReportsSettings;
            _commandProducer = commandProducer;
            _addressTransactionsReportMetadataRepository = addressTransactionsReportMetadataRepository;
        }

        [HttpPost]
        public async Task<CommandResult> CreateReport([FromBody]AddressTransactionsReportsRequest input)
        {
            if (!ModelState.IsValid)
            {
                return CommandResultBuilder.Fail(ModelState.GetErrorsList().ToArray());
            }

            if (!BitcoinAddressHelper.IsAddress(input.BitcoinAddress, _bcnReportsSettings.UsedNetwork()))
            {
                return CommandResultBuilder.Fail("Invalid base58 address string.");
            }

            await _addressTransactionsReportMetadataRepository.InsertOrReplace(ReportMetadata.Create(input.BitcoinAddress, queuedAt: DateTime.UtcNow));
            await _commandProducer.CreateCommand(input.BitcoinAddress, input.Email);
            
            return CommandResultBuilder.Ok();
        }

        [HttpGet]
        public async Task<IEnumerable<AddressReportMetadataViewModel>> GetReports()
        {
            var result = await _addressTransactionsReportMetadataRepository.GetAll();

            return result.Select(AddressReportMetadataViewModel.Create).ToList().OrderByDescending(p => p.QueuedAt);
        }

        [HttpGet("{address}")]
        public async Task<AddressReportMetadataViewModel> GetReport(string address)
        {
            var result = await _addressTransactionsReportMetadataRepository.Get(address);

            return result != null ? AddressReportMetadataViewModel.Create(result) : null;
        }
    }
}
