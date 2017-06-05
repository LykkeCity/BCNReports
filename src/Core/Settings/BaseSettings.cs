using System.ComponentModel.DataAnnotations;

namespace Core.Settings
{
    public class BaseSettings
    {
        [Required]
        public DbSettings Db { get; set; }

        [Required]
        public string NinjaUrl { get; set; }

        [Required]
        public string Network { get; set; }

        [Required]
        public string BlockChainExplolerUrl { get; set; }
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