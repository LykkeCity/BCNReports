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
using QBitNinja.Client;
using QBitNinja.Client.Models;
using Web.Helpers;

namespace Lykke.Service.BcnReports.Controllers
{
    [Route("api/[controller]")]
    public class BlockTransactionsReportsController:Controller
    {
        private readonly IBlockReportCommandProducer _commandProducer;
        private readonly IBlockTransactionsReportMetadataRepository _reportMetadataRepository;
        private readonly QBitNinjaClient _bitNinjaClient;

        public BlockTransactionsReportsController(IBlockReportCommandProducer commandProducer,
            IBlockTransactionsReportMetadataRepository reportMetadataRepository, 
            QBitNinjaClient bitNinjaClient)
        {
            _commandProducer = commandProducer;
            _reportMetadataRepository = reportMetadataRepository;
            _bitNinjaClient = bitNinjaClient;
        }


        [HttpPost]
        public async Task<CommandResult> CreateReport([FromBody]BlockTransactionsReportsRequest input)
        {
            if (!ModelState.IsValid)
            {
                return CommandResultBuilder.Fail(ModelState.GetErrorsList().ToArray());
            }

            if (!await IsBlock(input.Block))
            {
                return CommandResultBuilder.Fail($"Block {input.Block} not found");
            }

            await _reportMetadataRepository.InsertOrReplace(ReportMetadata.Create(input.Block, queuedAt: DateTime.UtcNow));
            await _commandProducer.CreateCommand(input.Block, input.Email);
            
            return CommandResultBuilder.Ok();
        }

        [HttpGet]
        public async Task<IEnumerable<BlockReportMetadataViewModel>> GetReports()
        {
            var result = await _reportMetadataRepository.GetAll();

            return result.Select(BlockReportMetadataViewModel.Create).ToList().OrderByDescending(p => p.QueuedAt);
        }

        [HttpGet("{block}")]
        public async Task<BlockReportMetadataViewModel> GetReport(string block)
        {
            var result = await _reportMetadataRepository.Get(block);

            return result != null ? BlockReportMetadataViewModel.Create(result) : null;
        }

        private async Task<bool> IsBlock(string blockFeature)
        {
            try
            {
                var block = await _bitNinjaClient.GetBlock(BlockFeature.Parse(blockFeature), headerOnly: true);
                return block != null;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}
