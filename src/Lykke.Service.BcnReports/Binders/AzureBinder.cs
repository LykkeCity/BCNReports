using System;
using Autofac;
using Autofac.Features.ResolveAnything;
using AzureRepositories;
using AzureStorage.Tables;
using Common;
using Common.Log;
using Lykke.Logs;
using Lykke.Service.BcnReports.AzureRepositories;
using Lykke.Service.BcnReports.Bindning;
using Lykke.Service.BcnReports.Core.Settings;
using Lykke.Service.BcnReports.Services;
using Lykke.SettingsReader;
using Lykke.SlackNotification.AzureQueue;
using Lykke.SlackNotifications;
using Microsoft.Extensions.DependencyInjection;

namespace Lykke.Service.BcnReports.Binders
{
    public class AzureBinder
    {
        public ContainerBuilder Bind(IReloadingManager<GeneralSettings> settings, IServiceCollection services, ILog log)
        {

            
            var consoleWriter = new ConsoleLWriter(Console.WriteLine);

            var ioc = new ContainerBuilder();

            ioc.RegisterInstance(log);
            ioc.RegisterInstance(consoleWriter).As<IConsole>();


            InitContainer(ioc, settings, log);

            return ioc;
        }

        private void InitContainer(ContainerBuilder ioc, IReloadingManager<GeneralSettings> settings, ILog log)
        {
            
#if DEBUG
            log.WriteInfoAsync("BcnReports Web", "App start", null, $"BcnReportsSettings : {settings.ToJson()}").Wait();
#else
            log.WriteInfoAsync("BcnReports Web", "App start", null, $"BcnReportsSettings : private").Wait();
#endif

            ioc.RegisterInstance(settings);
            ioc.RegisterInstance(settings.CurrentValue.BcnReports);

            ioc.BindCommonServices(settings.CurrentValue, log);
            ioc.BindAzure(settings, log);
            ioc.BindAzure(settings, log);
            ioc.BindBackgroundJobs(settings.CurrentValue, log);
            
        }
    }
}
