using Autofac;
using Common.Cache;
using Common.Log;
using LkeServices.Asset;
using Lykke.Service.BcnReports.Core.Address;
using Lykke.Service.BcnReports.Core.AddressTransactionReport;
using Lykke.Service.BcnReports.Core.Asset;
using Lykke.Service.BcnReports.Core.AssetTransactionReport;
using Lykke.Service.BcnReports.Core.Block;
using Lykke.Service.BcnReports.Core.BlockTransactionsReport;
using Lykke.Service.BcnReports.Core.NinjaClient;
using Lykke.Service.BcnReports.Core.Services;
using Lykke.Service.BcnReports.Core.Settings;
using Lykke.Service.BcnReports.Core.Transaction;
using Lykke.Service.BcnReports.Core.Xlsx;
using Lykke.Service.BcnReports.Services.Address;
using Lykke.Service.BcnReports.Services.AddressTransactionReport;
using Lykke.Service.BcnReports.Services.AssetTransactionReport;
using Lykke.Service.BcnReports.Services.Block;
using Lykke.Service.BcnReports.Services.BlockTransactionsReport;
using Lykke.Service.BcnReports.Services.NinjaClient;
using Lykke.Service.BcnReports.Services.Settings;
using Lykke.Service.BcnReports.Services.Transaction;
using Lykke.Service.BcnReports.Services.Xlsx;
using Lykke.Service.EmailSender;
using NBitcoin;
using QBitNinja.Client;

namespace Lykke.Service.BcnReports.Services
{
    public static class SrvBinder
    {
        public static void BindCommonServices(this ContainerBuilder ioc, GeneralSettings settings, ILog log)
        {
            ioc.Register(p =>
            {
                var context = p.Resolve<IComponentContext>();
                return new AssetDefinitionService(settings.BcnReports, new MemoryCacheManager(), context.Resolve<IConsole>());
            }).As<IAssetDefinitionService>().SingleInstance();
            ioc.RegisterType<AssetTransactionsesService>().As<IAssetTransactionsService>();
            ioc.RegisterType<TransactionXlsxRenderer>().As<ITransactionXlsxRenderer>();
            ioc.RegisterType<TransactionService>().As<ITransactionService>().SingleInstance();
            ioc.RegisterType<BlockService>().As<IBlockService>().SingleInstance();
            ioc.RegisterType<AddressService>().As<IAddressService>();
            ioc.RegisterType<AddressTransactionReportService>().As<IAddressTransactionReportService>();
            ioc.RegisterType<AssetTransactionsReportService>().As<IAssetTransactionsReportService>();
            ioc.RegisterType<BlockTransactionsReportService>().As<IBlockTransactionsReportService>().SingleInstance();
            ioc.RegisterType<HealthService>().As<IHealthService>();
            ioc.RegisterType<StartupManager>().As<IStartupManager>();
            ioc.RegisterType<ShutdownManager>().As<IShutdownManager>();

            ioc.Register(p => settings.BcnReports.UsedNetwork()).As<Network>();

            ioc.Register(p => new EmailSenderClient(settings.EmailSender.ServiceUrl, log)).AsSelf();

            ioc.RegisterType<NinjaClientFactory>().As<INinjaClientFactory>().SingleInstance();
        }
    }
}
