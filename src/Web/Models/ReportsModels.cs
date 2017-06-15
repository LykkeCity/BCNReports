using System;
using System.ComponentModel.DataAnnotations;
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


    public class AssetTransactionsReportsRequest
    {
        [Required]
        public string Asset { get; set; }

        [EmailAddress]
        public string Email { get; set; }
    }

    public class AddressReportMetadataViewModel
    {
        public string FileUrl { get; set; }

        public string Status { get; set; }
        public string Address { get; set; }
        public string LastError { get; set; }

        public DateTime? Started { get; set; }
        public DateTime? Finished { get; set; }
        public DateTime QueuedAt { get; set; }

        public static AddressReportMetadataViewModel Create(IBaseReportMetadata source)
        {
            return new AddressReportMetadataViewModel
            {
                Address = source.Id,
                FileUrl = source.FileUrl,
                Finished = source.Finished,
                Status = source.Status.ToString(),
                LastError = source.LastError,
                Started = source.Started,
                QueuedAt = source.QueuedAt
            };
        }
    }

    public class AssetReportMetadataViewModel
    {
        public string FileUrl { get; set; }

        public string Status { get; set; }
        public string AssetId { get; set; }
        public string LastError { get; set; }

        public DateTime? Started { get; set; }
        public DateTime? Finished { get; set; }
        public DateTime QueuedAt { get; set; }

        public static AssetReportMetadataViewModel Create(IBaseReportMetadata source)
        {
            return new AssetReportMetadataViewModel
            {
                AssetId = source.Id,
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
