using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Core.AddressTransactionReport;
using Core.Settings;
using LkeServices.BitcoinHelpers;
using LkeServices.Settings;
using Microsoft.AspNetCore.Mvc;
using Web.Helpers;
using Web.Models;

namespace Web.Controllers
{
    [Route("api/[controller]")]
    public class AddressTransactionsReportsController:Controller
    {
        private readonly IAddressXlsxService _addressXlsxService;
        private readonly BaseSettings _baseSettings;

        public AddressTransactionsReportsController(IAddressXlsxService addressXlsxService, 
            BaseSettings baseSettings)
        {
            _addressXlsxService = addressXlsxService;
            _baseSettings = baseSettings;
        }

        [HttpPost]
        public async Task<CommandResult> CreateReport([FromBody]AddressTransactionsReportsRequest input)
        {
            if (!ModelState.IsValid)
            {
                return CommandResultBuilder.Fail(ModelState.GetErrorsList().ToArray());
            }

            if (!BitcoinAddressHelper.IsAddress(input.Address, _baseSettings.UsedNetwork()))
            {
                return CommandResultBuilder.Fail("Invalid base58 address string.");
            }

            //todo write command except to generate stream 
            var str = await _addressXlsxService.GetTransactionsReport(input.Address);

            using (var fileStream = System.IO.File.Create("./test.xlsx"))
            {
                str.Seek(0, SeekOrigin.Begin);
                str.CopyTo(fileStream);

                fileStream.Flush(true);
            }
            

        
            return CommandResultBuilder.Ok();
        }
    }
}
