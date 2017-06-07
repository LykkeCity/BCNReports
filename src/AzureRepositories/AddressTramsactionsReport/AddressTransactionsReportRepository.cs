using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using AzureStorage;
using Common.Log;
using Core.AddressTramsactionsReport;

namespace AzureRepositories.AddressTramsactionsReport
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

    public class AddressTransactionsReportRepository: IAddressTransactionsReportRepository
    {
        private const string Container = "address-transaction-reports";
        private readonly IBlobStorage _blobStorage;
        private readonly ILog _log;

        public AddressTransactionsReportRepository(ILog log, IBlobStorage blobStorage)
        {
            _log = log;
            _blobStorage = blobStorage;
        }

        public async Task<ISaveResult> Save(string address, Stream data)
        {
            data.Position = 0;

            var key = GetKeyName(address);
            await _blobStorage.SaveBlobAsync(Container, key, data);

            var url = _blobStorage.GetBlobUrl(Container, key);
            return SaveResult.Ok(url);
        }

        private string GetKeyName(string address)
        {
            return address + ".xlsx";
        }
    }
}
