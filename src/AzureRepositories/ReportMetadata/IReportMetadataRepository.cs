using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace AzureRepositories.ReportMetadata
{
    public interface IReportMetadata
    {
        string FileUrl { get; }

        ReportStatus Status { get; }

        string Address { get; }
        
    }

    public class ReportMetadata:IReportMetadata
    {
        public string FileUrl { get; set; }
        public ReportStatus Status { get; set; }
        public string Address { get; set; }
        
    }

    public enum ReportStatus
    {
        Waiting,
        Processing,
        Failed,
        Done
    }

    public interface IReportMetadataRepository
    {
        Task<IReportMetadata> Get(string address);
    }
}
