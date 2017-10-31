using Lykke.Service.BcnReports.Core.NinjaClient;
using Lykke.Service.BcnReports.Core.Settings;
using Lykke.Service.BcnReports.Services.Settings;
using QBitNinja.Client;

namespace Lykke.Service.BcnReports.Services.NinjaClient
{
    public class NinjaClientFactory: INinjaClientFactory
    {
        private readonly BcnReportsSettings _bcnReportsSettings;

        public NinjaClientFactory(BcnReportsSettings bcnReportsSettings)
        {
            _bcnReportsSettings = bcnReportsSettings;
        }

        public QBitNinjaClient GetClient()
        {
            return new QBitNinjaClient(_bcnReportsSettings.NinjaUrl, _bcnReportsSettings.UsedNetwork())
            {
                Colored = true
            };
        }
    }
}
