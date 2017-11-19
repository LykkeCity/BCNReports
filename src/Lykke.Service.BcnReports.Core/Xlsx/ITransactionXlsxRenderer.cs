using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace Lykke.Service.BcnReports.Core.Xlsx
{
    public interface IXlsxTransactionsReportData
    {
        IEnumerable<IXlsxTransactionInputOutput> TransactionInputOutputs { get; }
    }

    public interface IXlsxTransactionInputOutput
    {
        string TransactionHash { get; }

        string BlockHash { get; }

        DateTime? BlockDate { get; }

        int? Index { get; }
        string Address { get; }
        string ColoredAddress { get; }

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

    public interface ITransactionXlsxRenderer
    {
        Task<Stream> RenderTransactionReport(IXlsxTransactionsReportData data);
    }
}
