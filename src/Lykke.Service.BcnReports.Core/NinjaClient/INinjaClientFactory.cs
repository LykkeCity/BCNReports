using QBitNinja.Client;

namespace Lykke.Service.BcnReports.Core.NinjaClient
{
    public interface INinjaClientFactory
    {
        QBitNinjaClient GetClient();
    }
}
