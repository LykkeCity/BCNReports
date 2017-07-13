using System;
using Core.Settings;
using NBitcoin;

namespace LkeServices.Settings
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
