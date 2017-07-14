using System.ComponentModel.DataAnnotations;

namespace Core.Settings
{
    public class BcnReportsSettings
    {
        [Required]
        public DbSettings Db { get; set; }

        [Required]
        public string NinjaUrl { get; set; }

        [Required]
        public string Network { get; set; }

        [Required]
        public string BlockChainExplolerUrl { get; set; }



        [Required]
        public int NinjaTransactionsMaxConcurrentRequestCount { get; set; }


        public int TimeoutMinutesOnGettingNinjaTransactionsList { get; set; } = 5;
    }

    public class DbSettings
    {
        [Required]
        public string DataConnString { get; set; }

        [Required]
        public string SharedConnString { get; set; }
        [Required]
        public string LogsConnString { get; set; }
    }


}