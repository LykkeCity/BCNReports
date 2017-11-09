using System.ComponentModel.DataAnnotations;

namespace Lykke.Service.BcnReports.Core.Settings
{
    public class GeneralSettings
    {
        public BcnReportsSettings BcnReports { get; set; }


        public EmailSenderSettings EmailSender { get; set; }

        

        public SlackNotificationSettings SlackNotifications { get; set; }
    }

    public class EmailSenderSettings
    {

        public string ServiceUrl { get; set; }
    }

    public class SlackNotificationSettings
    {

        public AzureQueueSettings AzureQueue { get; set; }
    }

    public class AzureQueueSettings
    {
        public string ConnectionString { get; set; }

        public string QueueName { get; set; }
    }

}
