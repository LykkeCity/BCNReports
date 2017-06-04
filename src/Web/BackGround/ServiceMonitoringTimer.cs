using System;
using System.Threading.Tasks;
using Common;
using Common.Log;
using Core.ServiceMonitoring;

namespace Web.BackGround
{
    public class ServiceMonitoringTimer : TimerPeriod
    {
        private readonly IServiceMonitoringRepository _serviceMonitoringRepository;

        private const string ServiceName = "BcnReports";
        public ServiceMonitoringTimer(IServiceMonitoringRepository serviceMonitoringRepository, ILog log = null) : base(ServiceName, 30000, log)
        {
            _serviceMonitoringRepository = serviceMonitoringRepository;
        }

        public override async Task Execute()
        {
            var now = DateTime.UtcNow;

            var record = new MonitoringRecord
            {
                DateTime = now,
                ServiceName = ServiceName,
                Version = Microsoft.Extensions.PlatformAbstractions.PlatformServices.Default.Application.ApplicationVersion
            };

            await _serviceMonitoringRepository.UpdateOrCreate(record);
        }
    }
}
