using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Core.ReportMetadata
{
    public interface IReportMetadata
    {
        string FileUrl { get; }

        ReportStatus Status { get; }

        string Address { get; }

        string LastError { get; }

        DateTime? Started { get; }

        DateTime? Finished { get; }

    }

    public class ReportMetadata:IReportMetadata
    {
        public string FileUrl { get; set; }
        public ReportStatus Status { get; set; }
        public string Address { get; set; }
        public string LastError { get; set; }
        public DateTime? Started { get; set; }
        public DateTime? Finished { get; set; }

        public static ReportMetadata Create(string address, 
            ReportStatus status = ReportStatus.Queued, 
            string fileUrl = null, 
            string lastError = null,
            DateTime? started = null,
            DateTime? finished = null)
        {
            return new ReportMetadata
            {
                Address = address,
                Status = status,
                FileUrl = fileUrl,
                LastError = lastError,
                Finished = finished,
                Started = started
            };
        }
    }

    public enum ReportStatus
    {
        Queued,
        Processing,
        Failed,
        Done
    }

    public interface IReportMetadataRepository
    {
        Task<IReportMetadata> Get(string address);
        Task<IEnumerable<IReportMetadata>> GetAll();
        Task InsertOrReplace(IReportMetadata reportMetadata);
        Task SetStatus(string address, ReportStatus status);
        Task SetProcessing(string address);
        Task SetDone(string address, string fileUrl);
        Task SetError(string address, string errorDescr);
    }
}
