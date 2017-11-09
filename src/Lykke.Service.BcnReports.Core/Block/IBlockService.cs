using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using QBitNinja.Client.Models;

namespace Lykke.Service.BcnReports.Core.Block
{
    public interface IBlockService
    {
        Task<GetBlockResponse> GetBlock(BlockFeature id);
    }
}
