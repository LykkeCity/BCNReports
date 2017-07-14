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
using Lykke.SlackNotification.AzureQueue;
using Lykke.SlackNotifications;
using Microsoft.Extensions.DependencyInjection;

namespace Web.Binders
{
    public class AzureBinder
    {
        public ContainerBuilder Bind(GeneralSettings settings, IServiceCollection services)
        {
            var logToTable = new LogToTable(new AzureTableStorage<LogEntity>(settings.BcnReports.Db.LogsConnString, "BcnReportsWebError", null),
                                            new AzureTableStorage<LogEntity>(settings.BcnReports.Db.LogsConnString, "BcnReportsWebWarning", null),
                                            new AzureTableStorage<LogEntity>(settings.BcnReports.Db.LogsConnString, "BcnReportsWebInfo", null));
            var log = new LogToTableAndConsole(logToTable, new LogToConsole());

            var slackService = services
                .UseSlackNotificationsSenderViaAzureQueue(
                    new Lykke.AzureQueueIntegration.AzureQueueSettings
                    {
                        ConnectionString = settings.SlackNotifications.AzureQueue.ConnectionString,
                        QueueName = settings.SlackNotifications.AzureQueue.QueueName
                    }, log);
            
            var consoleWriter = new ConsoleLWriter(Console.WriteLine);

            var ioc = new ContainerBuilder();

            ioc.RegisterInstance(log);
            ioc.RegisterInstance(consoleWriter).As<IConsole>();
            ioc.RegisterInstance(slackService).As<ISlackNotificationsSender>().SingleInstance();


            InitContainer(ioc, settings, log);

            return ioc;
        }

        private void InitContainer(ContainerBuilder ioc, GeneralSettings settings, ILog log)
        {
            
#if DEBUG
            log.WriteInfoAsync("BcnReports Web", "App start", null, $"BcnReportsSettings : {settings.ToJson()}").Wait();
#else
            log.WriteInfoAsync("BcnReports Web", "App start", null, $"BcnReportsSettings : private").Wait();
#endif

            ioc.RegisterInstance(settings);
            ioc.RegisterInstance(settings.BcnReports);

            ioc.BindCommonServices(settings, log);
            ioc.BindAzure(settings, log);
            ioc.BindAzure(settings, log);
            ioc.BindBackgroundJobs(settings, log);

            ioc.RegisterSource(new AnyConcreteTypeNotAlreadyRegisteredSource());
        }        
    }
}
