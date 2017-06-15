using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AzureStorage;
using Core.ReportMetadata;
using Microsoft.WindowsAzure.Storage.Table;

namespace AzureRepositories.ReportMetadata
{
    public class BaseReportMetadataEntity : TableEntity, IBaseReportMetadata
    {
        public string FileUrl { get; set; }
        ReportStatus IBaseReportMetadata.Status => (ReportStatus)Enum.Parse(typeof(ReportStatus), Status);

        public string Status { get; set; }
        public string Id { get; set; }
        public string LastError { get; set; }
        public DateTime QueuedAt { get; set; }

        public DateTime? Started { get; set; }
        public DateTime? Finished { get; set; }

        public static BaseReportMetadataEntity Create(IBaseReportMetadata source)
        {
            return new BaseReportMetadataEntity
            {
                Status = source.Status.ToString(),
                Id = source.Id,
                FileUrl = source.FileUrl,
                LastError = source.LastError,
                Started = source.Started,
                Finished = source.Finished,
                PartitionKey = GeneratePartitionKey(),
                RowKey = GenerateRowKey(source.Id),
                QueuedAt = source.QueuedAt
            };
        }

        public static string GeneratePartitionKey()
        {
            return "BCNR";
        }

        public static string GenerateRowKey(string id)
        {
            return id;
        }
    }


    public abstract class BaseReportMetadataRepository:IBaseReportMetadataRepository
    {
        private readonly INoSQLTableStorage<BaseReportMetadataEntity> _storage;

        protected BaseReportMetadataRepository(INoSQLTableStorage<BaseReportMetadataEntity> storage)
        {
            _storage = storage;
        }

        public async Task<IBaseReportMetadata> Get(string address)
        {
            return await _storage.GetDataAsync(BaseReportMetadataEntity.GeneratePartitionKey(),
                BaseReportMetadataEntity.GenerateRowKey(address));
        }

        public async Task<IEnumerable<IBaseReportMetadata>> GetAll()
        {
            return await _storage.GetDataAsync(BaseReportMetadataEntity.GeneratePartitionKey());
        }

        public Task InsertOrReplace(IBaseReportMetadata reportMetadata)
        {
            return _storage.InsertOrReplaceAsync(BaseReportMetadataEntity.Create(reportMetadata));
        }

        public Task SetStatus(string address, ReportStatus status)
        {
            return _storage.ReplaceAsync(BaseReportMetadataEntity.GeneratePartitionKey(),
                BaseReportMetadataEntity.GenerateRowKey(address),
                p =>
                {
                    p.Status = status.ToString();

                    return p;
                });
        }

        public Task SetProcessing(string address)
        {
            return _storage.ReplaceAsync(BaseReportMetadataEntity.GeneratePartitionKey(),
                BaseReportMetadataEntity.GenerateRowKey(address),
                p =>
                {
                    p.Status = ReportStatus.Processing.ToString();
                    p.Started = DateTime.UtcNow;

                    return p;
                });
        }

        public Task SetDone(string address, string fileUrl)
        {
            return _storage.ReplaceAsync(BaseReportMetadataEntity.GeneratePartitionKey(),
                BaseReportMetadataEntity.GenerateRowKey(address),
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
            return _storage.ReplaceAsync(BaseReportMetadataEntity.GeneratePartitionKey(),
                BaseReportMetadataEntity.GenerateRowKey(address),
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
