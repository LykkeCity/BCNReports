using Autofac;
using Common.Log;
using LkeServices.Address;
using LkeServices.Asset;
using Lykke.Service.BcnReports.Core.Address;
using Lykke.Service.BcnReports.Core.AddressTransactionReport;
using Lykke.Service.BcnReports.Core.Asset;
using Lykke.Service.BcnReports.Core.AssetTransactionReport;
using Lykke.Service.BcnReports.Core.Services;
using Lykke.Service.BcnReports.Core.Settings;
using Lykke.Service.BcnReports.Core.Transaction;
using Lykke.Service.BcnReports.Services.AddressTransactionReport;
using Lykke.Service.BcnReports.Services.AssetTransactionReport;
using Lykke.Service.BcnReports.Services.Settings;
using Lykke.Service.BcnReports.Services.Transaction;
using Lykke.Service.BcnReports.Services.Xlsx;
using Lykke.Service.EmailSender;
using QBitNinja.Client;

namespace Lykke.Service.BcnReports.Services
{
    public static class SrvBinder
    {
        public static void BindCommonServices(this ContainerBuilder ioc, GeneralSettings settings, ILog log)
        {
            ioc.RegisterType<AssetDefinitionService>().As<IAssetDefinitionService>();
            ioc.RegisterType<AssetTransactionsesService>().As<IAssetTransactionsService>();
            ioc.RegisterType<TransactionXlsxRenderer>().As<ITransactionXlsxRenderer>();
            ioc.RegisterType<TransactionService>().As<ITransactionService>().SingleInstance();
            ioc.RegisterType<AddressService>().As<IAddressService>();
            ioc.RegisterType<AddressTransactionReportService>().As<IAddressTransactionReportService>();
            ioc.RegisterType<AssetTransactionsReportService>().As<IAssetTransactionsReportService>();
            ioc.RegisterType<HealthService>().As<IHealthService>();
            ioc.RegisterType<StartupManager>().As<IStartupManager>();
            ioc.RegisterType<ShutdownManager>().As<IShutdownManager>();

            ioc.Register(p => new EmailSenderClient(settings.EmailSender.ServiceUrl, log)).AsSelf();

            ioc.Register(p => new QBitNinjaClient(settings.BcnReports.NinjaUrl, settings.BcnReports.UsedNetwork()){Colored = true});
        }
    }
}
