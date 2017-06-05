using System;
using System.IO;
using System.Threading.Tasks;
using Core.AddressTransactionReport;

namespace LkeServices.AddressTransactionReport
{
    public class AddressXlsxRenderer:IAddressXlsxRenderer
    {
        public Task<Stream> RenderTransactionReport(IXlsxTransactionsReportData data)
        {
            throw new NotImplementedException();
        }
    }
}
