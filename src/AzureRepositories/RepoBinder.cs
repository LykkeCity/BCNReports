using Autofac;
using AzureRepositories.AlertNotifications;
using AzureRepositories.ServiceMonitoring;
using AzureStorage.Queue;
using AzureStorage.Tables;
using Common.Log;
using Core.AlertNotifications;
using Core.Queue;
using Core.ServiceMonitoring;
using Core.Settings;
using LkeServices.ReportsCommands;
using Lykke.JobTriggers.Abstractions;

namespace AzureRepositories
{
    public static class RepoBinder
    {
        public static void BindAzure(this ContainerBuilder ioc, BaseSettings settings, ILog log)
        {
            ioc.BindRepo(settings, log);
            ioc.BindQueue(settings);
        }

        private static void BindRepo(this ContainerBuilder ioc, BaseSettings settings, ILog log)
        {


            ioc.RegisterInstance(new ServiceMonitoringRepository(new AzureTableStorage<MonitoringRecordEntity>(settings.Db.SharedConnString, "Monitoring", log)))
                .As<IServiceMonitoringRepository>();

        }

        private static void BindQueue(this ContainerBuilder ioc, BaseSettings settings)
        {
            ioc.Register(p => new SlackNotificationsProducer(
                    new AzureQueueExt(settings.Db.SharedConnString, QueueNames.SlackNotifications)))
                .As<ISlackNotificationsProducer>();

            ioc.Register(p => new SlackNotificationsProducer(
                    new AzureQueueExt(settings.Db.SharedConnString, QueueNames.SlackNotifications)))
                .As<IPoisionQueueNotifier>();

            ioc.Register(p => new ReportCommandProducer(new AzureQueueExt(settings.Db.DataConnString, QueueNames.AddressTransactionsReport)))
                .As<IReportCommandProducer>();
        }
    }
}
