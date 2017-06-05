using System;
using Core.Settings;
using NBitcoin;

namespace LkeServices.Settings
{
    public static class BaseSettingsHelper
    {
        public static Network UsedNetwork(this BaseSettings baseSettings)
        {
            try
            {
                return Network.GetNetwork(baseSettings.Network);
            }
            catch (Exception)
            {
                return Network.Main;
            }
        }
    }
}
