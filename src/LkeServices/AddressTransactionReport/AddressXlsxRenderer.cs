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
                    {"Tx Hash", new XlsxCellBuilder(p => p.TransactionHash, width: 75) },

                    {"Block No.", new XlsxCellBuilder(p =>p.BlockHash, width: 75) },
                    {"Type", new XlsxCellBuilder(p => p.CoinType.ToString(), width: 10) },
                    {"No.", new XlsxCellBuilder(p => p.Index, width: 10) },
                    {"Address", new XlsxCellBuilder(p => p.Address, width: 75) },
                    {"Btc value", new XlsxCellBuilder(p => p.BtcValue.ToStringBtcFormat(), width: 50) },
                    {"Coloured Asset", new XlsxCellBuilder(p => p.ColouredAssetName, width: 50) },
                    {"Coloured Asset Value", new XlsxCellBuilder(p => p.ColouredAssetValue, width: 50) }
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

                    ws.Column(columnCounter).Width = cellBuilder.Width;
                    ws.Cells[firstRow + 1, columnCounter]
                        .LoadFromCollection(data.TransactionInputOutputs.Select(p => cellBuilder.GetValue(p)));
                    columnCounter++;
                }
                
                //ws.Cells.AutoFitColumns(); //not worked in docker... 
                var result = new MemoryStream(package.GetAsByteArray());
                result.Position = 0;
                return result; 
            }

            
        }

        class XlsxCellBuilder
        {
            private readonly Func<IXlsxTransactionInputOutput, object> _valueSelector;
            

            public XlsxCellBuilder(Func<IXlsxTransactionInputOutput, object> valueSelector, double width = 10)
            {
                _valueSelector = valueSelector;
                Width = width;
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

            public double Width { get; set; }
        }

    }
}
