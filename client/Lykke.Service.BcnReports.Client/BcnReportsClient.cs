using System;
using Common.Log;

namespace Lykke.Service.BcnReports.Client
{
    public class BcnReportsClient : IBcnReportsClient, IDisposable
    {
        private readonly ILog _log;

        public BcnReportsClient(string serviceUrl, ILog log)
        {
            _log = log;
        }

        public void Dispose()
        {
            //if (_service == null)
            //    return;
            //_service.Dispose();
            //_service = null;
        }
    }
}
