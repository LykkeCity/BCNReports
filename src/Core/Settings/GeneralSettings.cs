using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;
using Lykke.EmailSenderProducer;
using Lykke.EmailSenderProducer.Interfaces;

namespace Core.Settings
{
    public class GeneralSettings
    {
        [Required]
        public BcnReportsSettings BcnReports { get; set; }

        [Required]
        public ServiceBusEmailSettings EmailServiceBus { get; set; }
    }

    public class ServiceBusEmailSettings : IServiceBusEmailSettings
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
