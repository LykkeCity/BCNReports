using System;
using System.IO;
using System.Threading.Tasks;
using Core.AddressTransactionReport;

namespace LkeServices.AddressTransactionReport
{
    public class AddressXlsxRenderer:IAddressXlsxRenderer
    {
        public async Task<Stream> RenderTransactionReport(IXlsxTransactionsReportData data)
        {
            return Stream.Null;
        }
    }
}
