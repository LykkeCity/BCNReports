using Autofac;
using Common.Log;
using Core;
using Core.Address;
using Core.AddressTransactionReport;
using Core.Asset;
using Core.AssetTransactionReport;
using Core.Settings;
using Core.Transaction;
using LkeServices.Address;
using LkeServices.AddressTransactionReport;
using LkeServices.Asset;
using LkeServices.AssetTransactionReport;
using LkeServices.Settings;
using LkeServices.Transaction;
using LkeServices.Xlsx;
using Lykke.Service.EmailSender;
using QBitNinja.Client;

namespace LkeServices
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

            ioc.Register(p => new EmailSenderClient(settings.EmailSender.ServiceUrl, log)).AsSelf();

            ioc.Register(p => new QBitNinjaClient(settings.BcnReports.NinjaUrl, settings.BcnReports.UsedNetwork()){Colored = true});
        }
    }
}
