using System.ComponentModel.DataAnnotations;
using Lykke.SettingsReader.Attributes;

namespace Lykke.Service.BcnReports.Core.Settings
{
    public class BcnReportsSettings
    {

        public DbSettings Db { get; set; }


        public string NinjaUrl { get; set; }


        public string Network { get; set; }


        public string BlockChainExplolerUrl { get; set; }


        public int NinjaTransactionsMaxConcurrentRequestCount { get; set; }

        [Optional]
        public int TimeoutMinutesOnGettingNinjaTransactionsList { get; set; } = 5;

        [Optional]
        public int MaxBlockCountPerCommand { get; set; } = 100;
    }

    public class DbSettings
    {
        public string DataConnString { get; set; }
        
        public string LogsConnString { get; set; }
    }

}
