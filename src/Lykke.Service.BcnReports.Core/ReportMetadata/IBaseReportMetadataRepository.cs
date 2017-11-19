using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Lykke.Service.BcnReports.Core.ReportMetadata
{
    public interface IBaseReportMetadata
    {
        string FileUrl { get; set; }

        ReportStatus Status { get; }

        string Id { get; }

        string LastError { get; }

        DateTime QueuedAt { get; }
        DateTime? Started { get; }

        DateTime? Finished { get; }

    }

    public class ReportMetadata : IBaseReportMetadata
    {
        public string FileUrl { get; set; }
        public ReportStatus Status { get; set; }
        public string Id { get; set; }
        public string LastError { get; set; }
        public DateTime QueuedAt { get; set; }
        public DateTime? Started { get; set; }
        public DateTime? Finished { get; set; }

        public static ReportMetadata Create(string id,
            DateTime queuedAt,
            ReportStatus status = ReportStatus.Queued,
            string fileUrl = null,
            string lastError = null,
            DateTime? started = null,
            DateTime? finished = null)
        {
            return new ReportMetadata
            {
                Id = id,
                Status = status,
                FileUrl = fileUrl,
                LastError = lastError,
                Finished = finished,
                Started = started,
                QueuedAt = queuedAt
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

    public interface IBaseReportMetadataRepository
    {
        Task<IBaseReportMetadata> Get(string id);
        Task<IEnumerable<IBaseReportMetadata>> GetAll();
        Task InsertOrReplace(IBaseReportMetadata reportMetadata);
        Task SetStatus(string id, ReportStatus status);
        Task SetProcessing(string id);
        Task SetDone(string id, string fileUrl);
        Task SetError(string id, string errorDescr);
        string GeneratePartitionKey(string id);
    }
}
