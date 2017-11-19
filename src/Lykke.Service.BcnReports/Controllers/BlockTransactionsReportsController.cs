using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Common;
using Lykke.Service.BcnReports.Core.NinjaClient;
using Lykke.Service.BcnReports.Core.Queue;
using Lykke.Service.BcnReports.Core.ReportMetadata;
using Lykke.Service.BcnReports.Core.Settings;
using Lykke.Service.BcnReports.Models;
using Microsoft.AspNetCore.Mvc;
using MoreLinq;
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
        private readonly IBlockRangeReportCommandProducer _blockRangeReportCommandProducer;
        private readonly BcnReportsSettings _bcnReportsSettings;

        public BlockTransactionsReportsController(IBlockReportCommandProducer commandProducer,
            IBlockTransactionsReportMetadataRepository reportMetadataRepository,
            INinjaClientFactory bitNinjaClient, 
            BcnReportsSettings bcnReportsSettings,
            IBlockRangeReportCommandProducer blockRangeReportCommandProducer)
        {
            _commandProducer = commandProducer;
            _reportMetadataRepository = reportMetadataRepository;
            _bitNinjaClient = bitNinjaClient;
            _bcnReportsSettings = bcnReportsSettings;
            _blockRangeReportCommandProducer = blockRangeReportCommandProducer;
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
        public async Task<CommandResult> CreateRangeReport([FromQuery]int minBlock, [FromQuery]int maxBlock, [FromQuery]int batch = 100)
        {
            await _blockRangeReportCommandProducer.CreateRangeReport(minBlock, maxBlock, batch);

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
