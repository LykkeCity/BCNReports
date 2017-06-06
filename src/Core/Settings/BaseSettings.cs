using System.ComponentModel.DataAnnotations;
using Lykke.EmailSenderProducer.Interfaces;

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

        [Required]
        public ServiceBusEmailSettings ServiceBusEmailSettings { get; set; }

        [Required]
        public int NinjaTransactionsMaxConcurrentRequest { get; set; }
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

    public class ServiceBusEmailSettings: IServiceBusEmailSettings
    {
        [Required]
        public string NamespaceUrl { get; set; }
        [Required]
        public string PolicyName { get; set; }
        [Required]
        public string Key { get; set; }
        [Required]
        public string QueueName { get; set; }
    }
}