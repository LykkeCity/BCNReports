using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace Core.AddressTramsactionsReport
{
    public interface IAddressTransactionsReportRepository
    {
        Task<ISaveResult> Save(string address, Stream data);
    }

    public interface ISaveResult
    {
        string Url { get; }
        bool Saved { get; }
    }
}
