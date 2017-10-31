using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Common.Extensions;
using Common.Log;
using Lykke.Service.BcnReports.AzureRepositories.Helpers;
using Lykke.Service.BcnReports.Core.Block;
using Lykke.Service.BcnReports.Core.NinjaClient;
using Lykke.Service.BcnReports.Core.Settings;
using QBitNinja.Client;
using QBitNinja.Client.Models;

namespace Lykke.Service.BcnReports.Services.Block
{
    public class BlockService:IBlockService
    {
        private readonly SemaphoreSlim _globalSemaphore;
        private readonly INinjaClientFactory _bitNinjaClient;
        private readonly ILog _log;

        public BlockService(BcnReportsSettings bcnReportsSettings,
            INinjaClientFactory bitNinjaClient, ILog log)
        {
            _bitNinjaClient = bitNinjaClient;
            _log = log;

            _globalSemaphore = new SemaphoreSlim(bcnReportsSettings.NinjaBlocksMaxConcurrentRequestCount);
        }

        public async Task<GetBlockResponse> GetBlock(BlockFeature id)
        {
            try
            {
                await _globalSemaphore.WaitAsync();

                await _log.WriteMonitorAsync(nameof(BlockService), nameof(GetBlock), $"{id} started");

                return await Retry.Try(() => _bitNinjaClient.GetClient().GetBlock(id).WithTimeout(60*1000),
                    component:nameof(GetBlock),
                    tryCount: 10,
                    logger: _log,
                    secondsToWaitOnFail: 5);
            }
            catch (Exception e)
            {
                await _log.WriteErrorAsync(nameof(BlockService), nameof(GetBlock), e);
                throw;
            }
            finally
            {
                _globalSemaphore.Release(1);

                await _log.WriteMonitorAsync(nameof(BlockService), nameof(GetBlock), $"{id} done");
            }
        }
    }
}
