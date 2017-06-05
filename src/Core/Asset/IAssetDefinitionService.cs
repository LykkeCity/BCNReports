using System.Collections.Generic;
using System.Threading.Tasks;

namespace Core.Asset
{
    public interface IAssetDefinition
    {
        IEnumerable<string> AssetIds { get; }

        string ContactUrl { get; }

        string NameShort { get; }

        string Name { get; }

        string Issuer { get; }

        string Description { get; }

        string DescriptionMime { get; }

        string Type { get; }

        int Divisibility { get; }

        bool LinkToWebsite { get; }

        string IconUrl { get; set; }

        string ImageUrl { get; set; }

        string Version { get; }
    }

    public interface IAssetDefinitionService
    {
        Task<IDictionary<string, IAssetDefinition>> GetAssetDefinitionsAsync();
    }
}
