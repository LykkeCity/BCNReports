using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Core.AddressTransactionReport;
using LkeServices.BitcoinHelpers;
using NBitcoin;
using OfficeOpenXml;

namespace LkeServices.AddressTransactionReport
{
    public class AddressXlsxRenderer:IAddressXlsxRenderer
    {
        public async Task<Stream> RenderTransactionReport(IXlsxTransactionsReportData data)
        {

            using (var package = new ExcelPackage())
            {
               var  ws = package.Workbook.Worksheets.Add("Address Transactions");

                var config = new Dictionary<string, XlsxCellBuilder>
                {
                    {"Tx Hash", new XlsxCellBuilder(p => p.TransactionHash) },

                    {"Block No.", new XlsxCellBuilder(p =>p.BlockHash) },
                    {"Type", new XlsxCellBuilder(p => p.CoinType.ToString()) },
                    {"No.", new XlsxCellBuilder(p => p.Index) },
                    {"Address", new XlsxCellBuilder(p => p.Address) },
                    {"Btc value", new XlsxCellBuilder(p => p.BtcValue.ToStringBtcFormat()) },
                    {"Coloured Asset", new XlsxCellBuilder(p => p.ColouredAssetName) },
                    {"Coloured Asset Value", new XlsxCellBuilder(p => p.ColouredAssetValue) }
                };

                var columnCounter = 1;


                var title = ws.Cells[1, 1];
                title.Value = "Export from Lykke Blockchain Explorer";
                title.Style.Font.Size = 25;
                title.Style.Font.Bold = true;

                const  int firstRow = 6;
                foreach (var headerKey in config.Keys)
                {

                    var headerCell = ws.Cells[firstRow, columnCounter];
                    headerCell.Value = headerKey;
                    headerCell.Style.Font.Bold = true;
                    var cellBuilder = config[headerKey];

                    ws.Cells[firstRow + 1, columnCounter]
                        .LoadFromCollection(data.TransactionInputOutputs.Select(p => cellBuilder.GetValue(p)));
                    columnCounter++;
                }
                
                //ws.Cells.AutoFitColumns();
                var result = new MemoryStream(package.GetAsByteArray());
                result.Position = 0;
                return result; 
            }

            
        }

        class XlsxCellBuilder
        {
            private readonly Func<IXlsxTransactionInputOutput, object> _valueSelector;

            public XlsxCellBuilder(Func<IXlsxTransactionInputOutput, object> valueSelector)
            {
                _valueSelector = valueSelector;
            }

            public string GetValue(IXlsxTransactionInputOutput source)
            {
                var result = _valueSelector(source);
                if (result != null)
                {
                    return result.ToString();
                }

                return "";
            }
        }

    }
}
