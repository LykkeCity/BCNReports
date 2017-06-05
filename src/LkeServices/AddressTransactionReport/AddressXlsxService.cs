using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Core.AddressTransactionReport;

namespace LkeServices.AddressTransactionReport
{
    public class XlsxTransactionsReportData : IXlsxTransactionsReportData
    {
        public IEnumerable<IXlsxTransactionInputOutput> TransactionInputOutputs { get; set; }

        public class XlsxTransactionInputOutput : IXlsxTransactionInputOutput
        {
            public string TransactionHash { get; set; }
            public string BlockHash { get; set; }
            public int Index { get; set; }
            public string Address { get; set; }
            public double BtcValue { get; set; }
            public string ColouredAssetName { get; set; }
            public double ColouredAssetValue { get; set; }
        }

        public class AddressXlsxService : IAddressXlsxService
        {
            private readonly IAddressXlsxRenderer _addressXlsxRenderer;

            public AddressXlsxService(IAddressXlsxRenderer addressXlsxRenderer)
            {
                _addressXlsxRenderer = addressXlsxRenderer;
            }

            public Task<Stream> GetTransactionsReport(string addressId)
            {
                throw new NotImplementedException();
            }
        }
    }
}
