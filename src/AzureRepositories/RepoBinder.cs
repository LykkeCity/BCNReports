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
using Core.Settings;
using Lykke.JobTriggers.Abstractions;
using Lykke.MonitoringServiceApiCaller;
using IMonitoringService = Core.ServiceMonitoring.IMonitoringService;

namespace AzureRepositories
{
    public static class RepoBinder
    {
        public static void BindAzure(this ContainerBuilder ioc, GeneralSettings settings, ILog log)
        {
            ioc.BindRepo(settings, log);
            ioc.BindQueue(settings);
        }

        private static void BindRepo(this ContainerBuilder ioc, GeneralSettings settings, ILog log)
        {
            ioc.RegisterInstance(new MonitoringService(new MonitoringServiceFacade(settings.MonitoringServiceClient.MonitoringServiceUrl)))
                .As<IMonitoringService>();

            ioc.RegisterInstance(new AddressTransactionsReportMetadataRepository(new AzureTableStorage<BaseReportMetadataEntity>(settings.BcnReports.Db.DataConnString, "AddressTransactionReportMetadata", log)))
                .As<IAddressTransactionsReportMetadataRepository>();
            
            ioc.RegisterInstance(new AddressTransactionsReportStorage(log, new AzureBlobStorage(settings.BcnReports.Db.DataConnString)))
                .As<IAddressTransactionsReportStorage>();

            ioc.RegisterInstance(new AssetTransactionsReportMetadataRepository(new AzureTableStorage<BaseReportMetadataEntity>(settings.BcnReports.Db.DataConnString, "AssetTransactionReportMetadata", log)))
                .As<IAssetTransactionsReportMetadataRepository>();

            ioc.RegisterInstance(new AssetTransactionsReportStorage(log, new AzureBlobStorage(settings.BcnReports.Db.DataConnString)))
                .As<IAssetTransactionsReportStorage>();
        }

        private static void BindQueue(this ContainerBuilder ioc, GeneralSettings settings)
        {
            ioc.RegisterType<SlackNotificationsProducer>()
                .As<ISlackNotificationsProducer>();

            ioc.RegisterType<SlackNotificationsProducer>()
                .As<IPoisionQueueNotifier>();

            ioc.Register(p => new AddressReportCommandProducer(new AzureQueueExt(settings.BcnReports.Db.DataConnString, QueueNames.AddressTransactionsReport)))
                .As<IAddressReportCommandProducer>();

            ioc.Register(p => new AssetReportCommandProducer(new AzureQueueExt(settings.BcnReports.Db.DataConnString, QueueNames.AssetTransactionsReport)))
                .As<IAssetReportCommandProducer>();
        }
    }
}
