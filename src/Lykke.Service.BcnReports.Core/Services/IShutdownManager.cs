using System.Threading.Tasks;

namespace Lykke.Service.BcnReports.Core.Services
{
    public interface IShutdownManager
    {
        Task StopAsync();
    }
}