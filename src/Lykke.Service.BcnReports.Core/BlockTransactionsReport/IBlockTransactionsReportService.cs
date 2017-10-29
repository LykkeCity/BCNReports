using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace Lykke.Service.BcnReports.Core.BlockTransactionsReport
{
    public interface IBlockTransactionsReportService
    {
        Task<Stream> GetTransactionsReport(string blockId);
    }
}
