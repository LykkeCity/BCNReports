using System;
using System.Threading.Tasks;
using Common.Log;

namespace AzureRepositories.Helpers
{
    public class Retry
    {
        public static async Task<T> Try<T>(Func<Task<T>> action, Func<Exception, bool> exceptionFilter, int tryCount, ILog logger, int secondsToWaitOnFail = 0)
        {
            int @try = 0;
            while (true)
            {
                try
                {
                    return await action();
                }
                catch (Exception ex)
                {
                    @try++;
                    if (!exceptionFilter(ex) || @try >= tryCount)
                        throw;
                    await logger.WriteErrorAsync("Retry", "Try", null, ex);
                    await Task.Delay(secondsToWaitOnFail * 1000);
                }
            }
        }
    }
}
