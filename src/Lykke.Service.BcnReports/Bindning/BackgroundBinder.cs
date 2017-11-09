using Autofac;
using Common.Log;
using Lykke.JobTriggers.Extenstions;
using Lykke.Service.BcnReports.Core.Settings;
using Lykke.Service.BcnReports.QueueHandlers;

namespace Lykke.Service.BcnReports.Bindning
{
    public static class BackgroundBinder
    {
        public static void BindBackgroundJobs(this ContainerBuilder ioc, 
            GeneralSettings settings,
            ILog log)
        {
            ioc.RegisterType<AddressTransactionsQueueFunctions>().AsSelf().SingleInstance();
            ioc.RegisterType<AssetTransactionsQueueFunctions>().AsSelf().SingleInstance();

            ioc.AddTriggers(pool =>
            {
                // default connection must be initialized
                pool.AddDefaultConnection(settings.BcnReports.Db.DataConnString);
            });
        }
    }
}
