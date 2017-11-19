using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AzureStorage;
using Lykke.Service.BcnReports.Core.ReportMetadata;
using Microsoft.WindowsAzure.Storage.Table;

namespace Lykke.Service.BcnReports.AzureRepositories.ReportMetadata
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

        public static BaseReportMetadataEntity Create(IBaseReportMetadata source, string partition)
        {
            return new BaseReportMetadataEntity
            {
                Status = source.Status.ToString(),
                Id = source.Id,
                FileUrl = source.FileUrl,
                LastError = source.LastError,
                Started = source.Started,
                Finished = source.Finished,
                PartitionKey = partition,
                RowKey = GenerateRowKey(source.Id),
                QueuedAt = source.QueuedAt
            };
        }
        

        public static string GenerateRowKey(string id)
        {
            return id;
        }
    }


    public abstract class BaseReportMetadataRepository:IBaseReportMetadataRepository
    {
        protected readonly INoSQLTableStorage<BaseReportMetadataEntity> Storage;

        protected BaseReportMetadataRepository(INoSQLTableStorage<BaseReportMetadataEntity> storage)
        {
            Storage = storage;
        }

        public async Task<IBaseReportMetadata> Get(string id)
        {
            return await Storage.GetDataAsync(GeneratePartitionKey(id),
                BaseReportMetadataEntity.GenerateRowKey(id));
        }

        public async Task<IEnumerable<IBaseReportMetadata>> GetAll()
        {
            return await Storage.GetDataAsync();
        }

        public virtual Task InsertOrReplace(IBaseReportMetadata reportMetadata)
        {
            return Storage.InsertOrReplaceAsync(BaseReportMetadataEntity.Create(reportMetadata, GeneratePartitionKey(reportMetadata.Id)));
        }

        public Task SetStatus(string id, ReportStatus status)
        {
            return Storage.ReplaceAsync(GeneratePartitionKey(id),
                BaseReportMetadataEntity.GenerateRowKey(id),
                p =>
                {
                    p.Status = status.ToString();

                    return p;
                });
        }

        public Task SetProcessing(string id)
        {
            return Storage.ReplaceAsync(GeneratePartitionKey(id),
                BaseReportMetadataEntity.GenerateRowKey(id),
                p =>
                {
                    p.Status = ReportStatus.Processing.ToString();
                    p.Started = DateTime.UtcNow;

                    return p;
                });
        }

        public Task SetDone(string id, string fileUrl)
        {
            return Storage.ReplaceAsync(GeneratePartitionKey(id),
                BaseReportMetadataEntity.GenerateRowKey(id),
                p =>
                {
                    p.Status = ReportStatus.Done.ToString();
                    p.Finished = DateTime.UtcNow;
                    p.FileUrl = fileUrl;

                    return p;
                });
        }

        public Task SetError(string id, string errorDescr)
        {
            return Storage.ReplaceAsync(GeneratePartitionKey(id),
                BaseReportMetadataEntity.GenerateRowKey(id),
                p =>
                {
                    p.Status = ReportStatus.Failed.ToString();
                    p.Finished = DateTime.UtcNow;
                    p.LastError = errorDescr;

                    return p;
                });
        }

        public virtual string GeneratePartitionKey(string id)
        {
            return "BCNR";
        }
    }
}
