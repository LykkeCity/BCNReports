using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using AzureRepositories.Settings;
using AzureRepositories.Settings.Validation;
using BackGroundJobs.TimerFunctions;
using Common.Log;
using Core.Settings;
using Lykke.AzureQueueIntegration;
using Lykke.JobTriggers.Triggers;
using Lykke.SlackNotification.AzureQueue;
using Lykke.SlackNotifications;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Swashbuckle.Swagger.Model;
using Web.Binders;

namespace Web
{
    public class Startup
    {
        public IContainer ApplicationContainer { get; set; }
        public IServiceProvider ServiceProvider { get; set; }

        public Startup(IHostingEnvironment env)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true)
                .AddEnvironmentVariables();
            Configuration = builder.Build();
        }

        private GeneralSettings GetSettings()
        {
#if DEBUG
            var settings = GeneralSettingsReader.ReadGeneralSettingsLocal<GeneralSettings>("../../settings.json");
#else
            var settings = GeneralSettingsReader.ReadGeneralSettings<GeneralSettings>(Configuration["SettingsUrl"]);
#endif

            GeneralSettingsValidator.Validate(settings);

            return settings;
        }

        public IConfigurationRoot Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public IServiceProvider ConfigureServices(IServiceCollection services)
        {
            var settings = GetSettings();


            // Add framework services.
            services.AddMvc();


            services.AddSwaggerGen(options =>
            {
                options.SingleApiVersion(new Info
                {
                    Version = "v1",
                    Title = "BcnReports_Api"
                });
                options.DescribeAllEnumsAsStrings();
            });



            var builder = new AzureBinder().Bind(settings, services);

            builder.Populate(services);


            ApplicationContainer = builder.Build();
            ServiceProvider = new AutofacServiceProvider(ApplicationContainer);

            return ServiceProvider;
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory, IApplicationLifetime appLifetime)
        {
            loggerFactory.AddConsole(Configuration.GetSection("Logging"));
            loggerFactory.AddDebug();


            if (!env.IsProduction())
            {
                app.UseDeveloperExceptionPage();
            }


            app.UseSwagger();
            app.UseSwaggerUi("swagger/ui/index");

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");
                
            });


            var triggerHost = new TriggerHost(ServiceProvider);
            triggerHost.ProvideAssembly(typeof(MonitoringFunctions).GetTypeInfo().Assembly);

            appLifetime.ApplicationStarted.Register(() =>
                {
                    new Thread(() =>
                    {
                        triggerHost.Start().Wait();
                    }).Start();
                }
            );

            appLifetime.ApplicationStopping.Register(() =>
                {
                    triggerHost.Cancel();
                }
            );
        }
    }
}
