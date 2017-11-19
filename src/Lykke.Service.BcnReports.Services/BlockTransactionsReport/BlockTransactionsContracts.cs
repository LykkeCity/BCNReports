using System;
using System.Collections.Generic;
using System.Text;
using Lykke.Service.BcnReports.Services.Address;
using Newtonsoft.Json;

namespace Lykke.Service.BcnReports.Services.BlockTransactionsReport
{
    public class TransactionListContract
    {
        [JsonProperty("continuation")]
        public string ContinuationToken { get; set; }

        [JsonProperty("operations")]
        public TransactionListItemContract[] Transactions { get; set; }

        [JsonProperty("conflictedOperations")]
        public object[] ConflictedOperations { get; set; }
    }

    public class TransactionListItemContract
    {
        [JsonProperty("amount", NullValueHandling = NullValueHandling.Ignore)]
        public double? Amount { get; set; }

        [JsonProperty("confirmations")]
        public int Confirmations { get; set; }

        [JsonProperty("height")]
        public int? Height { get; set; }

        [JsonProperty("blockId")]
        public string BlockId { get; set; }

        [JsonProperty("transactionId")]
        public string TxId { get; set; }

        [JsonProperty("receivedCoins")]
        public InOutContract[] Received { get; set; }

        [JsonProperty("spentCoins")]
        public InOutContract[] Spent { get; set; }

        [JsonProperty("fees", NullValueHandling = NullValueHandling.Ignore)]
        public double? Fees { get; set; }
    }
}
