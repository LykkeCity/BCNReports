using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using AzureRepositories.Settings;
using AzureRepositories.Settings.Validation;
using BackGroundJobs.TimerFunctions;
using Core.Settings;
using Lykke.JobTriggers.Triggers;
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

        private BaseSettings GetSettings()
        {
#if DEBUG
            var settings = GeneralSettingsReader.ReadGeneralSettingsLocal<BaseSettings>("../../settings.json");
#else
            var generalSettings = GeneralSettingsReader.ReadGeneralSettings<GeneralSettings>(Configuration.GetConnectionString("Settings"));
            var settings = generalSettings?.BcnReports;
#endif

            GeneralSettingsValidator.Validate(settings);

            return settings;
        }

        public IConfigurationRoot Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public IServiceProvider ConfigureServices(IServiceCollection services)
        {
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


            var settings = GetSettings();

            var builder = new AzureBinder().Bind(settings);
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
                    triggerHost.Start().Wait();
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
