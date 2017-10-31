using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Common;
using LkeServices.BitcoinHelpers;
using Lykke.Service.BcnReports.Core.NinjaClient;
using Lykke.Service.BcnReports.Core.Queue;
using Lykke.Service.BcnReports.Core.ReportMetadata;
using Lykke.Service.BcnReports.Core.Settings;
using Lykke.Service.BcnReports.Models;
using Lykke.Service.BcnReports.Services.Settings;
using Microsoft.AspNetCore.Mvc;
using MoreLinq;
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
        private readonly INinjaClientFactory _bitNinjaClient;
        private readonly BcnReportsSettings _bcnReportsSettings;

        public BlockTransactionsReportsController(IBlockReportCommandProducer commandProducer,
            IBlockTransactionsReportMetadataRepository reportMetadataRepository,
            INinjaClientFactory bitNinjaClient, 
            BcnReportsSettings bcnReportsSettings)
        {
            _commandProducer = commandProducer;
            _reportMetadataRepository = reportMetadataRepository;
            _bitNinjaClient = bitNinjaClient;
            _bcnReportsSettings = bcnReportsSettings;
        }


        [HttpPost]
        public async Task<CommandResult> CreateReport([FromBody]BlockTransactionsReportsRequest input)
        {


            if (!ModelState.IsValid)
            {
                return CommandResultBuilder.Fail(ModelState.GetErrorsList().ToArray());
            }

            if (!input.Blocks.Any())
            {
                return CommandResultBuilder.Fail("0 block count");
            }

            if (input.Blocks.Length > _bcnReportsSettings.MaxBlockCountPerCommand)
            {
                return CommandResultBuilder.Fail($"Maximum block count per request exceeded: {_bcnReportsSettings.MaxBlockCountPerCommand}");
            }

            var blockExistence = (await input.Blocks.SelectAsync(IsBlock)).ToList();

            if (blockExistence.Any(p => !p.exists))
            {
                var notFoundBlocks = blockExistence.Where(p => !p.exists).Select(p => p.block);
                return CommandResultBuilder.Fail($"Blocks {string.Join(", ", notFoundBlocks)} not found");
            }

            await _commandProducer.CreateCommand(input.Blocks, input.Email);

            foreach (var block in input.Blocks) {
                await _reportMetadataRepository.InsertOrReplace(ReportMetadata.Create(block, queuedAt: DateTime.UtcNow));  
            }


            return CommandResultBuilder.Ok();
        }

        [HttpPost("range")]
        public async Task<CommandResult> CreateRangeReport([FromQuery]int minBlock, [FromQuery]int maxBlock)
        {
            var list = Enumerable.Range(minBlock, maxBlock - minBlock + 1);

            foreach (var bath in list.Batch(10))
            {

                await _commandProducer.CreateCommand(bath.Select(p => p.ToString()), null);
                foreach (var block in bath)
                {
                    await _reportMetadataRepository.InsertOrReplace(ReportMetadata.Create(block.ToString(), queuedAt: DateTime.UtcNow));
                }
            }


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

        private async Task<(string block, bool exists)> IsBlock(string blockFeature)
        {
            try
            {
                var block = await _bitNinjaClient.GetClient().GetBlock(BlockFeature.Parse(blockFeature), headerOnly: true);
                return (blockFeature, block != null);
            }
            catch (Exception)
            {
                return (blockFeature, false);
            }
        }
        
    }
}
