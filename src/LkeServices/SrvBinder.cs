﻿using Autofac;
using AzureStorage.Queue;
using Common.Log;
using Core.Address;
using Core.AddressTransactionReport;
using Core.Asset;
using Core.Queue;
using Core.Settings;
using LkeServices.AddressTransactionReport;
using LkeServices.Asset;
using LkeServices.Settings;
using Lykke.EmailSenderProducer;
using QBitNinja.Client;

namespace LkeServices
{
    public static class SrvBinder
    {
        public static void BindCommonServices(this ContainerBuilder ioc, BaseSettings settings, ILog log)
        {
            ioc.RegisterType<AssetDefinitionService>().As<IAssetDefinitionService>();
            ioc.RegisterType<AddressXlsxRenderer>().As<IAddressXlsxRenderer>();
            ioc.RegisterType<AddressService>().As<IAddressService>();
            ioc.RegisterType<AddressXlsxService>().As<IAddressXlsxService>().SingleInstance();


            ioc.Register(p => new EmailSenderProducer(settings.ServiceBusEmailSettings,  log)).AsSelf();

            ioc.Register(p => new QBitNinjaClient(settings.NinjaUrl, settings.UsedNetwork()){Colored = true});
        }
    }
}
