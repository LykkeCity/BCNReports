using Autofac;
using AzureRepositories.AddressTramsactionsReport;
using AzureRepositories.AlertNotifications;
using AzureRepositories.ReportMetadata;
using AzureRepositories.ReportsCommands;
using AzureRepositories.ServiceMonitoring;
using AzureStorage.Blob;
using AzureStorage.Queue;
using AzureStorage.Tables;
using Common.Log;
using Core.AddressTramsactionsReport;
using Core.AlertNotifications;
using Core.Queue;
using Core.ReportMetadata;
using Core.ServiceMonitoring;
using Core.Settings;
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

            ioc.RegisterInstance(new ReportMetadataRepository(new AzureTableStorage<ReportMetadataEntity>(settings.Db.DataConnString, "ReportMetadata", log)))
                .As<IReportMetadataRepository>();

            ioc.RegisterInstance(new AddressTransactionsReportRepository(log, new AzureBlobStorage(settings.Db.DataConnString)))
                .As<IAddressTransactionsReportRepository>();
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
