using System;
using Lykke.Service.BcnReports.Core.Settings;
using NBitcoin;

namespace Lykke.Service.BcnReports.Services.Settings
{
    public static class BaseSettingsHelper
    {
        public static Network UsedNetwork(this BcnReportsSettings bcnReportsSettings)
        {
            try
            {
                return Network.GetNetwork(bcnReportsSettings.Network);
            }
            catch (Exception)
            {
                return Network.Main;
            }
        }
    }
}
