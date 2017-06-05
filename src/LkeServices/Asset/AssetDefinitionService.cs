using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Core.Asset;
using Core.Settings;
using Flurl;
using Flurl.Http;

namespace LkeServices.Asset
{

    public class AssetDefinitionContract
    {
        public IEnumerable<string> AssetIds { get; set; }

        public string ContactUrl { get; set; }

        public string NameShort { get; set; }

        public string Name { get; set; }

        public string Issuer { get; set; }

        public string Description { get; set; }

        public string DescriptionMime { get; set; }

        public string Type { get; set; }

        public int Divisibility { get; set; }

        public bool LinkToWebsite { get; set; }

        public string IconUrl { get; set; }

        public string ImageUrl { get; set; }

        public string Version { get; set; }

        public bool IsVerified { get; set; }

        public double Score { get; set; }

        public string IssuerWebSite { get; set; }
    }

    public class AssetDefition : IAssetDefinition
    {
        public IEnumerable<string> AssetIds { get; set; }
        public string ContactUrl { get; set; }
        public string NameShort { get; set; }
        public string Name { get; set; }
        public string Issuer { get; set; }
        public string Description { get; set; }
        public string DescriptionMime { get; set; }
        public string Type { get; set; }
        public int Divisibility { get; set; }
        public bool LinkToWebsite { get; set; }
        public string IconUrl { get; set; }
        public string ImageUrl { get; set; }
        public string Version { get; set; }

        public static AssetDefition Create(AssetDefinitionContract source)
        {
            return new AssetDefition
            {
                AssetIds = source.AssetIds,
                ContactUrl = source.ContactUrl,
                Description = source.Description,
                DescriptionMime = source.DescriptionMime,
                Divisibility = source.Divisibility,
                IconUrl = source.IconUrl,
                ImageUrl = source.ImageUrl,
                Issuer = source.Issuer,
                LinkToWebsite = source.LinkToWebsite,
                Name = source.Name,
                NameShort = source.NameShort,
                Type = source.Type,
                Version = source.Version
            };
        }
    }

    public class AssetDefinitionService:IAssetDefinitionService
    {
        private readonly BaseSettings _baseSettings;

        public AssetDefinitionService(BaseSettings baseSettings)
        {
            _baseSettings = baseSettings;
        }

        public async Task<IDictionary<string, IAssetDefinition>> GetAssetDefinitionsAsync()
        {
            var resp = await _baseSettings.BlockChainExplolerUrl.AppendPathSegment("/api/assets").GetJsonAsync<List<AssetDefinitionContract>>();

            var result = new Dictionary<string, IAssetDefinition>();
            foreach (var assetContract in resp)
            {
                var assetResultModel = AssetDefition.Create(assetContract);
                foreach (var assetId in assetContract.AssetIds)
                {
                    result[assetId] = assetResultModel;
                }
            }

            return result;
        }
    }
}
