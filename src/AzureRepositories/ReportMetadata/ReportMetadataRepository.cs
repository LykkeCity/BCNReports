using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using AzureStorage;
using Core.ReportMetadata;
using Microsoft.WindowsAzure.Storage.Table;

namespace AzureRepositories
{
    public class ReportMetadataEntity : TableEntity, IReportMetadata
    {
        public string FileUrl { get; set; }
        ReportStatus IReportMetadata.Status => (ReportStatus)Enum.Parse(typeof(ReportStatus), Status);

        public string Status { get; set; }
        public string Address { get; set; }
        public string LastError { get; set; }

        public DateTime? Started { get; set; }
        public DateTime? Finished { get; set; }

        public static ReportMetadataEntity Create(IReportMetadata source)
        {
            return new ReportMetadataEntity
            {
                Status = source.Status.ToString(),
                Address = source.Address,
                FileUrl = source.FileUrl,
                LastError = source.LastError,
                Started = source.Started,
                Finished = source.Finished,
                PartitionKey = GeneratePartitionKey(),
                RowKey = GenerateRowKey(source.Address)
            };
        }

        public static string GeneratePartitionKey()
        {
            return "BCNR";
        }

        public static string GenerateRowKey(string address)
        {
            return address;
        }
    }

    public class ReportMetadataRepository: IReportMetadataRepository
    {
        private readonly INoSQLTableStorage<ReportMetadataEntity> _storage;

        public ReportMetadataRepository(INoSQLTableStorage<ReportMetadataEntity> storage)
        {
            _storage = storage;
        }

        public async Task<IReportMetadata> Get(string address)
        {
            return await _storage.GetDataAsync(ReportMetadataEntity.GeneratePartitionKey(),
                ReportMetadataEntity.GenerateRowKey(address));
        }

        public async Task<IEnumerable<IReportMetadata>> Get()
        {
            return await _storage.GetDataAsync(ReportMetadataEntity.GeneratePartitionKey());
        }

        public Task InsertOrReplace(IReportMetadata reportMetadata)
        {
            return _storage.InsertOrReplaceAsync(ReportMetadataEntity.Create(reportMetadata));
        }

        public Task SetStatus(string address, ReportStatus status)
        {
            return _storage.ReplaceAsync(ReportMetadataEntity.GeneratePartitionKey(),
                ReportMetadataEntity.GenerateRowKey(address),
                p =>
                {
                    p.Status = status.ToString();

                    return p;
                });
        }

        public Task SetProcessing(string address)
        {
            return _storage.ReplaceAsync(ReportMetadataEntity.GeneratePartitionKey(),
                ReportMetadataEntity.GenerateRowKey(address),
                p =>
                {
                    p.Status = ReportStatus.Processing.ToString();
                    p.Started = DateTime.UtcNow;

                    return p;
                });
        }

        public Task SetDone(string address, string fileUrl)
        {
            return _storage.ReplaceAsync(ReportMetadataEntity.GeneratePartitionKey(),
                ReportMetadataEntity.GenerateRowKey(address),
                p =>
                {
                    p.Status = ReportStatus.Done.ToString();
                    p.Finished = DateTime.UtcNow;
                    p.FileUrl = fileUrl;

                    return p;
                });
        }

        public Task SetError(string address, string errorDescr)
        {
            return _storage.ReplaceAsync(ReportMetadataEntity.GeneratePartitionKey(),
                ReportMetadataEntity.GenerateRowKey(address),
                p =>
                {
                    p.Status = ReportStatus.Failed.ToString();
                    p.Finished = DateTime.UtcNow;
                    p.LastError = errorDescr;

                    return p;
                });
        }
    }
}
