using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Core.ServiceMonitoring
{

    public interface IMonitoringRecord
    {
        string ServiceName { get; }
        string Version { get; }
    }

    public class MonitoringRecord : IMonitoringRecord
    {


        public string ServiceName { get; set; }
        public string Version { get; set; }

        public static MonitoringRecord Create(string serviceName, string version)
        {
            return new MonitoringRecord
            {
                ServiceName = serviceName,
                Version = version
            };
        }
    }

    public interface IMonitoringService
    {
        Task WriteRecord(IMonitoringRecord record);
    }
}
