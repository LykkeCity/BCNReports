using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace Core.AddressTransactionReport
{
    public interface IXlsxTransactionsReportData
    {
        IEnumerable<IXlsxTransactionInputOutput> TransactionInputOutputs { get; }
    }

    public interface IXlsxTransactionInputOutput
    {
        string TransactionHash { get; }

        string BlockHash { get; }

        int Index { get; }
        string Address { get; }

        double BtcValue { get; }

        string ColouredAssetName { get; }

        double ColouredAssetValue { get; }

        CoinType CoinType { get; }
    }

    public enum CoinType
    {
        Input,
        Output,
        Fees
    }

    public interface IAddressXlsxRenderer
    {
        Task<Stream> RenderTransactionReport(IXlsxTransactionsReportData data);
    }
}
