namespace Lykke.Service.BcnReports.Core.Queue
{
    public static class QueueNames
    {
        public const string AddressTransactionsReport = "address-transactions-reports";
        public const string AssetTransactionsReport = "asset-transactions-reports";

#if DEBUG
        public const string BlockTransactionsReport = "block-transactions-reports-debug";
#endif
#if !DEBUG
                public const string BlockTransactionsReport = "block-transactions-reports";
#endif

        public const string SlackNotifications = "slack-notifications";
    }
}
