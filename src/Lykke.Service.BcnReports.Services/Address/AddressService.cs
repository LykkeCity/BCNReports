using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Common;
using Lykke.Service.BcnReports.Core.Address;
using Lykke.Service.BcnReports.Core.Settings;
using Newtonsoft.Json;

namespace Lykke.Service.BcnReports.Services.Address
{
    #region NinjaContracts

    public class AddressTransactionListContract
    {
        [JsonProperty("continuation")]
        public string ContinuationToken { get; set; }

        [JsonProperty("operations")]
        public AddressTransactionListItemContract[] Transactions { get; set; }
    }

    public class AddressTransactionListItemContract
    {
        [JsonProperty("transactionId")]
        public string TxId { get; set; }

        [JsonProperty("amount")]
        public double Amount { get; set; }

        [JsonProperty("confirmations")]
        public int Confirmations { get; set; }

        [JsonProperty("height")]
        public int? Height { get; set; }

        [JsonProperty("blockId")]
        public string BlockId { get; set; }

        [JsonProperty("receivedCoins")]
        public InOutContract[] Received { get; set; }

        [JsonProperty("spentCoins")]
        public InOutContract[] Spent { get; set; }
    }

    public class InOutContract
    {
        [JsonProperty("address")]
        public string Address { get; set; }

        [JsonProperty("transactionId")]
        public string TransactionId { get; set; }

        [JsonProperty("index")]
        public int Index { get; set; }

        [JsonProperty("value")]
        public double Value { get; set; }

        [JsonProperty("scriptPubKey")]
        public string ScriptPubKey { get; set; }

        [JsonProperty("assetId")]
        public string AssetId { get; set; }

        [JsonProperty("quantity")]
        public double Quantity { get; set; }
    }


    #endregion

    public class AddressTransaction : IAddressTransaction
    {
        public string TransactionId { get; set; }

        public static AddressTransaction Create(AddressTransactionListItemContract source)
        {
            return new AddressTransaction
            {
                TransactionId = source.TxId
            };
        }
    }

    public class AddressService:IAddressService
    {
        private readonly BcnReportsSettings _bcnReportsSettings;

        public AddressService(BcnReportsSettings bcnReportsSettings)
        {
            _bcnReportsSettings = bcnReportsSettings;
        }

        public async Task<IEnumerable<IAddressTransaction>> GetTransactionsForAddress(string address)
        {
            string continuation = null;
            var result = new List<IAddressTransaction>();
            using (var httpClient = new HttpClient())
            {
                do
                {
                    httpClient.Timeout = TimeSpan.FromMinutes(_bcnReportsSettings.TimeoutMinutesOnGettingNinjaTransactionsList);
                    httpClient.BaseAddress = new Uri(_bcnReportsSettings.NinjaUrl);

                    var url = $"/balances/{address}/?colored=true"
                              + (string.IsNullOrEmpty(continuation) ? "" : $"&continuation={continuation}");


                    var resp = await httpClient.GetAsync(url);

                    resp.EnsureSuccessStatusCode();

                    var respContent = (await resp.Content.ReadAsStringAsync()).DeserializeJson<AddressTransactionListContract>();

                    continuation = respContent?.ContinuationToken;

                    var txsContract = respContent?.Transactions ?? Enumerable.Empty<AddressTransactionListItemContract>();

                    result.AddRange(txsContract.Select(AddressTransaction.Create));

                } while (!string.IsNullOrEmpty(continuation));
            }

            return result;

        }
    }
}
