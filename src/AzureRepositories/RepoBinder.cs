using Autofac;
using AzureRepositories.AlertNotifications;
using AzureRepositories.ReportMetadata;
using AzureRepositories.ReportsCommands;
using AzureRepositories.ReportStorage;
using AzureRepositories.ServiceMonitoring;
using AzureStorage.Blob;
using AzureStorage.Queue;
using AzureStorage.Tables;
using Common.Log;
using Core.AlertNotifications;
using Core.Queue;
using Core.ReportMetadata;
using Core.ReportStorage;
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

            ioc.RegisterInstance(new AddressTransactionsReportMetadataRepository(new AzureTableStorage<BaseReportMetadataEntity>(settings.Db.DataConnString, "AddressTransactionReportMetadata", log)))
                .As<IAddressTransactionsReportMetadataRepository>();
            
            ioc.RegisterInstance(new AddressTransactionsReportStorage(log, new AzureBlobStorage(settings.Db.DataConnString)))
                .As<IAddressTransactionsReportStorage>();

            ioc.RegisterInstance(new AssetTransactionsReportMetadataRepository(new AzureTableStorage<BaseReportMetadataEntity>(settings.Db.DataConnString, "AssetTransactionReportMetadata", log)))
                .As<IAssetTransactionsReportMetadataRepository>();

            ioc.RegisterInstance(new AssetTransactionsReportStorage(log, new AzureBlobStorage(settings.Db.DataConnString)))
                .As<IAssetTransactionsReportStorage>();
        }

        private static void BindQueue(this ContainerBuilder ioc, BaseSettings settings)
        {
            ioc.Register(p => new SlackNotificationsProducer(
                    new AzureQueueExt(settings.Db.SharedConnString, QueueNames.SlackNotifications)))
                .As<ISlackNotificationsProducer>();

            ioc.Register(p => new SlackNotificationsProducer(
                    new AzureQueueExt(settings.Db.SharedConnString, QueueNames.SlackNotifications)))
                .As<IPoisionQueueNotifier>();

            ioc.Register(p => new AddressReportCommandProducer(new AzureQueueExt(settings.Db.DataConnString, QueueNames.AddressTransactionsReport)))
                .As<IAddressReportCommandProducer>();

            ioc.Register(p => new AssetReportCommandProducer(new AzureQueueExt(settings.Db.DataConnString, QueueNames.AssetTransactionsReport)))
                .As<IAssetReportCommandProducer>();
        }
    }
}
