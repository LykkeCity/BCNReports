using Autofac;
using BackGroundJobs.QueueHandlers;
using BackGroundJobs.TimerFunctions;
using Common.Log;
using Core.Settings;
using Lykke.JobTriggers.Abstractions;
using Lykke.JobTriggers.Extenstions;

namespace BackGroundJobs.Bindning
{
    public static class BackgroundBinder
    {
        public static void BindBackgroundJobs(this ContainerBuilder ioc, 
            BaseSettings settings,
            ILog log)
        {
            ioc.RegisterType<AddressTransactionsQueueFunctions>().AsSelf().SingleInstance();
            ioc.RegisterType<AssetTransactionsQueueFunctions>().AsSelf().SingleInstance();
            ioc.RegisterType<MonitoringFunctions>().AsSelf().SingleInstance();

            ioc.AddTriggers(pool =>
            {
                // default connection must be initialized
                pool.AddDefaultConnection(settings.Db.DataConnString);
            });
        }
    }
}
