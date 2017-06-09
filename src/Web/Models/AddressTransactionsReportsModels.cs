using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Core.AddressTramsactionsReport;
using Core.ReportMetadata;

namespace Web.Models
{
    public class AddressTransactionsReportsRequest
    {
        [Required]
        public string BitcoinAddress { get; set; }
        
        [EmailAddress]
        public string Email { get; set; }
    }

    public class ReportMetadataViewModel
    {
        public string FileUrl { get; set; }

        public string Status { get; set; }
        public string Address { get; set; }
        public string LastError { get; set; }

        public DateTime? Started { get; set; }
        public DateTime? Finished { get; set; }
        public DateTime QueuedAt { get; set; }

        public static ReportMetadataViewModel Create(IReportMetadata source)
        {
            return new ReportMetadataViewModel
            {
                Address = source.Address,
                FileUrl = source.FileUrl,
                Finished = source.Finished,
                Status = source.Status.ToString(),
                LastError = source.LastError,
                Started = source.Started,
                QueuedAt = source.QueuedAt
            };
        }
    }
}
