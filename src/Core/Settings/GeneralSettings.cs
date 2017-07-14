using System.ComponentModel.DataAnnotations;

namespace Core.Settings
{
    public class GeneralSettings
    {
        [Required]
        public BcnReportsSettings BcnReports { get; set; }

        [Required]
        public EmailSenderSettings EmailSender { get; set; }

        [Required]
        public MonitoringServiceClientSettings MonitoringServiceClient { get; set; }

        [Required]
        public SlackNotificationSettings SlackNotifications { get; set; }
    }

    public class EmailSenderSettings
    {
        [Required]
        public string ServiceUrl { get; set; }
    }

    public class SlackNotificationSettings
    {
        [Required]
        public AzureQueueSettings AzureQueue { get; set; }
    }

    public class AzureQueueSettings
    {
        public string ConnectionString { get; set; }

        public string QueueName { get; set; }
    }



    public class MonitoringServiceClientSettings
    {
        [Required]
        public string MonitoringServiceUrl { get; set; }
    }

}
