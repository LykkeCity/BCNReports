using System.ComponentModel.DataAnnotations;

namespace Core.Settings
{
    public class GeneralSettings
    {
        [Required]
        public BcnReportsSettings BcnReports { get; set; }

        [Required]
        public EmailSenderSettings EmailSender { get; set; }
    }

    public class EmailSenderSettings
    {
        [Required]
        public string ServiceUrl { get; set; }
    }
}
