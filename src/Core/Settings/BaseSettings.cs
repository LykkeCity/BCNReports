using System.ComponentModel.DataAnnotations;

namespace Core.Settings
{
    public class BaseSettings
    {
        [Required]
        public DbSettings Db { get; set; }
    }

    public class DbSettings
    {
        [Required]
        public string DataConnString { get; set; }

        [Required]
        public string SharedConnString { get; set; }
    }
}