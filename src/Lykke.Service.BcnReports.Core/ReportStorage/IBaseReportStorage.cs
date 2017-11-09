using System.IO;
using System.Threading.Tasks;

namespace Lykke.Service.BcnReports.Core.ReportStorage
{
    public interface ISaveResult
    {
        string Url { get; }
        bool Saved { get; }
    }

    public interface IBaseReportStorage
    {
        Task<ISaveResult> SaveXlsxReport(string address, Stream data);
    }
}
