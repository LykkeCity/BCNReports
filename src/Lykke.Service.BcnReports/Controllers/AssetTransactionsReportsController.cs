using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Lykke.Service.BcnReports.Core.Asset;
using Lykke.Service.BcnReports.Core.Queue;
using Lykke.Service.BcnReports.Core.ReportMetadata;
using Lykke.Service.BcnReports.Models;
using Microsoft.AspNetCore.Mvc;
using Web.Helpers;

namespace Lykke.Service.BcnReports.Controllers
{
    [Route("api/[controller]")]
    public class AssetTransactionsReportsController:Controller
    {
        private readonly IAssetReportCommandProducer _commandProducer;
        private readonly IAssetTransactionsReportMetadataRepository _reportMetadataRepository;
        private readonly IAssetDefinitionService _assetDefinitionService;

        public AssetTransactionsReportsController(
            IAssetReportCommandProducer commandProducer,
            IAssetTransactionsReportMetadataRepository reportMetadataRepository, 
            IAssetDefinitionService assetDefinitionService)
        {
            _commandProducer = commandProducer;
            _reportMetadataRepository = reportMetadataRepository;
            _assetDefinitionService = assetDefinitionService;
        }

        [HttpPost]
        public async Task<CommandResult> CreateReport([FromBody]AssetTransactionsReportsRequest input)
        {
            if (!ModelState.IsValid)
            {
                return CommandResultBuilder.Fail(ModelState.GetErrorsList().ToArray());
            }

            var assetDefinitions = await _assetDefinitionService.GetAssetDefinitionsAsync();

            if (!assetDefinitions.ContainsKey(input.Asset))
            {
                return CommandResultBuilder.Fail("Asset not found");
            }

            var asset = assetDefinitions[input.Asset];
            var assetId = asset.AssetIds.First();

            await _reportMetadataRepository.InsertOrReplace(ReportMetadata.Create(assetId, queuedAt: DateTime.UtcNow));
            await _commandProducer.CreateCommand(assetId, input.Email);
            
            return CommandResultBuilder.Ok();
        }

        [HttpGet]
        public async Task<IEnumerable<AssetReportMetadataViewModel>> GetReports()
        {
            var result = await _reportMetadataRepository.GetAll();

            return result.Select(AssetReportMetadataViewModel.Create).ToList().OrderByDescending(p => p.QueuedAt);
        }

        [HttpGet("{assetId}")]
        public async Task<AssetReportMetadataViewModel> GetReport(string assetId)
        {
            var result = await _reportMetadataRepository.Get(assetId);

            return result != null ? AssetReportMetadataViewModel.Create(result) : null;
        }
    }
}
