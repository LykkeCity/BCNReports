using System.IO;
using System.Threading.Tasks;
using AzureStorage;
using Common.Log;
using Core.ReportStorage;

namespace AzureRepositories.ReportStorage
{
    public class SaveResult : ISaveResult
    {
        public string Url { get; set; }
        public bool Saved { get; set; }

        public static SaveResult Ok(string url)
        {
            return new SaveResult
            {
                Url = url,
                Saved = true
            };
        }

        public static SaveResult Fail()
        {
            return new SaveResult
            {
                Saved = false
            };
        }
    }

    public abstract class BaseReportStorage : IBaseReportStorage
    {
        private readonly string _container;
        private readonly IBlobStorage _blobStorage;
        private readonly ILog _log;

        protected BaseReportStorage(string container, ILog log, IBlobStorage blobStorage)
        {
            _container = container;
            _log = log;
            _blobStorage = blobStorage;
        }

        public async Task<ISaveResult> Save(string address, Stream data)
        {
            data.Position = 0;

            var key = GetKeyName(address);
            await _blobStorage.SaveBlobAsync(_container, key, data);

            var url = _blobStorage.GetBlobUrl(_container, key);
            return SaveResult.Ok(url);
        }

        private string GetKeyName(string address)
        {
            return address + ".xlsx";
        }
    }
}
