using System;
using Autofac;
using Autofac.Features.ResolveAnything;
using AzureRepositories;
using AzureRepositories.Log;
using AzureStorage.Tables;
using BackGroundJobs.Bindning;
using Common;
using Common.Log;
using Core.Settings;
using LkeServices;

namespace Web.Binders
{
    public class AzureBinder
    {
        public ContainerBuilder Bind(GeneralSettings settings)
        {
            var logToTable = new LogToTable(new AzureTableStorage<LogEntity>(settings.BcnReports.Db.LogsConnString, "BcnReportsWebError", null),
                                            new AzureTableStorage<LogEntity>(settings.BcnReports.Db.LogsConnString, "BcnReportsWebWarning", null),
                                            new AzureTableStorage<LogEntity>(settings.BcnReports.Db.LogsConnString, "BcnReportsWebInfo", null));
            var log = new LogToTableAndConsole(logToTable, new LogToConsole());

            var ioc = new ContainerBuilder();

            var consoleWriter = new ConsoleLWriter(Console.WriteLine);

            ioc.RegisterInstance(consoleWriter).As<IConsole>();

            
            InitContainer(ioc, settings, log);

            return ioc;
        }

        private void InitContainer(ContainerBuilder ioc, GeneralSettings generalSettings, ILog log)
        {

            var settings = generalSettings.BcnReports;
#if DEBUG
            log.WriteInfoAsync("BcnReports Web", "App start", null, $"BcnReportsSettings : {settings.ToJson()}").Wait();
#else
            log.WriteInfoAsync("BcnReports Web", "App start", null, $"BcnReportsSettings : private").Wait();
#endif

            ioc.RegisterInstance(log);
            ioc.RegisterInstance(settings);

            ioc.BindCommonServices(settings, generalSettings.EmailServiceBus, log);
            ioc.BindAzure(settings, log);
            ioc.BindAzure(settings, log);
            ioc.BindBackgroundJobs(settings, log);

            ioc.RegisterSource(new AnyConcreteTypeNotAlreadyRegisteredSource());
        }        
    }
}
