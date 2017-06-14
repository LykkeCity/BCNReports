using System;
using System.Threading.Tasks;
using Common.Log;

namespace AzureRepositories.Helpers
{
    public class Retry
    {
        public static async Task<T> Try<T>(Func<Task<T>> action,  int tryCount, Func<Exception, bool> exceptionFilter = null, ILog logger = null, int secondsToWaitOnFail = 0)
        {
            int @try = 0;
            if (exceptionFilter == null)
            {
                exceptionFilter = p => true;
            }

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

                    if (logger != null)
                    {
                        await logger.WriteErrorAsync("Retry", "Try", null, ex);
                    }
                    await Task.Delay(secondsToWaitOnFail * 1000);
                }
            }
        }
    }
}
