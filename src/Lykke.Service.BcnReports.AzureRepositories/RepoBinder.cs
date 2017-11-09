using Autofac;
using AzureStorage.Blob;
using AzureStorage.Queue;
using AzureStorage.Tables;
using Common.Log;
using Lykke.JobTriggers.Abstractions;
using Lykke.Service.BcnReports.AzureRepositories.AlertNotifications;
using Lykke.Service.BcnReports.AzureRepositories.ReportMetadata;
using Lykke.Service.BcnReports.AzureRepositories.ReportsCommands;
using Lykke.Service.BcnReports.AzureRepositories.ReportStorage;
using Lykke.Service.BcnReports.Core.AlertNotifications;
using Lykke.Service.BcnReports.Core.Queue;
using Lykke.Service.BcnReports.Core.ReportMetadata;
using Lykke.Service.BcnReports.Core.ReportStorage;
using Lykke.Service.BcnReports.Core.Settings;
using Lykke.SettingsReader;

namespace Lykke.Service.BcnReports.AzureRepositories
{
    public static class RepoBinder
    {
        public static void BindAzure(this ContainerBuilder ioc, IReloadingManager<GeneralSettings> settings, ILog log)
        {
            ioc.BindRepo(settings, log);
            ioc.BindQueue(settings);
        }

        private static void BindRepo(this ContainerBuilder ioc, IReloadingManager<GeneralSettings> settings, ILog log)
        {

            var dataReloadingManager = settings.Nested(p => p.BcnReports.Db.DataConnString);

            ioc.RegisterInstance(new AddressTransactionsReportMetadataRepository(AzureTableStorage<BaseReportMetadataEntity>.Create(dataReloadingManager,"AddressTransactionReportMetadata", log)))
                .As<IAddressTransactionsReportMetadataRepository>();
            
            ioc.RegisterInstance(new AddressTransactionsReportStorage(log, AzureBlobStorage.Create(dataReloadingManager)))
                .As<IAddressTransactionsReportStorage>();

            ioc.RegisterInstance(new AssetTransactionsReportMetadataRepository(AzureTableStorage<BaseReportMetadataEntity>.Create(dataReloadingManager, "AssetTransactionReportMetadata", log)))
                .As<IAssetTransactionsReportMetadataRepository>();

            ioc.RegisterInstance(new AssetTransactionsReportStorage(log, AzureBlobStorage.Create(dataReloadingManager)))
                .As<IAssetTransactionsReportStorage>();

            ioc.RegisterInstance(new BlockTransactionsReportMetadataRepository(AzureTableStorage<BaseReportMetadataEntity>.Create(dataReloadingManager, "BlockTransactionReportMetadata", log)))
                .As<IBlockTransactionsReportMetadataRepository>();

            ioc.RegisterInstance(new BlockTransactionsReportStorage(log, AzureBlobStorage.Create(dataReloadingManager)))
                .As<IBlockTransactionsReportStorage>();
        }

        private static void BindQueue(this ContainerBuilder ioc, IReloadingManager<GeneralSettings> settings)
        {
            var dataReloadingManager = settings.Nested(p => p.BcnReports.Db.DataConnString);

            ioc.RegisterType<SlackNotificationsProducer>()
                .As<ISlackNotificationsProducer>();

            ioc.RegisterType<SlackNotificationsProducer>()
                .As<IPoisionQueueNotifier>();

            ioc.Register(p => new AddressReportCommandProducer(AzureQueueExt.Create(dataReloadingManager, QueueNames.AddressTransactionsReport)))
                .As<IAddressReportCommandProducer>();

            ioc.Register(p => new AssetReportCommandProducer(AzureQueueExt.Create(dataReloadingManager, QueueNames.AssetTransactionsReport)))
                .As<IAssetReportCommandProducer>();

            ioc.Register(p => new BlockReportCommandProducer(AzureQueueExt.Create(dataReloadingManager, QueueNames.BlockTransactionsReport)))
                .As<IBlockReportCommandProducer>();
        }
    }
}
