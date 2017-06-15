using System.IO;
using System.Threading.Tasks;

namespace Core.ReportStorage
{
    public interface ISaveResult
    {
        string Url { get; }
        bool Saved { get; }
    }

    public interface IBaseReportStorage
    {
        Task<ISaveResult> Save(string address, Stream data);
    }
}
