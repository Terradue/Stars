using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Stac;
using Stac.Extensions.Eo;
using Stac.Extensions.Projection;

namespace Terradue.Stars.Services.ThirdParty.Titiler
{
    public class TitilerService : IThirdPartyService
    {
        private readonly IOptions<TitilerConfiguration> options;
        private readonly ILogger<TitilerService> logger;

        private static string[] TITILER_VALID_TYPE = new string[7] {
            "image/vnd.stac.geotiff; cloud-optimized=true",
            "image/tiff; application=geotiff",
            "image/tiff",
            "image/x.geotiff",
            "image/jp2",
            "application/x-hdf5",
            "application/x-hdf",
        };

        private static string[] OVERVIEW_NAMES = new string[] { "red", "green", "blue", "nir", "pan" };

        public TitilerService(IOptions<TitilerConfiguration> options,
                                   ILogger<TitilerService> logger)
        {
            this.options = options;
            this.logger = logger;
        }

        public TitilerConfiguration Configuration => options.Value;

        public bool IsAvailable => Configuration != null;

        public Dictionary<string, StacAsset> SelectOverviewCombinationAssets(StacItem stacItem)
        {

            Dictionary<string, StacAsset> overviewAssets = stacItem.Assets
                                                            .Where(a => a.Value.Roles != null)
                                                            .Where(a => a.Value.Roles.Contains("overview") ||
                                                                        a.Value.Roles.Contains("visual"))
                                                            .Where(a => a.Key.Contains("overview"))
                                                            .Where(a => TITILER_VALID_TYPE.Contains(a.Value.MediaType?.MediaType))
                                                            .Take(1)
                                                            .ToDictionary(a => a.Key, a => a.Value);
            if (overviewAssets.Count == 1) return overviewAssets;

            overviewAssets = stacItem.Assets.Where(a => a.Value.Roles != null)
                                                            .Where(a => a.Value.Roles.Contains("overview") ||
                                                                        a.Value.Roles.Contains("visual"))
                                                            .OrderBy(a => a.Value.GetProperty<long>("size"))
                                                            .Where(a => TITILER_VALID_TYPE.Contains(a.Value.MediaType?.MediaType))
                                                            .Take(1)
                                                            .ToDictionary(a => a.Key, a => a.Value);
            if (overviewAssets.Count == 1) return overviewAssets;

            var projext = stacItem.ProjectionExtension();

            overviewAssets = stacItem.Assets
                .Where(a => a.Key.Equals("red", StringComparison.InvariantCultureIgnoreCase) ||
                            a.Key.Equals("green", StringComparison.InvariantCultureIgnoreCase) ||
                            a.Key.Equals("blue", StringComparison.InvariantCultureIgnoreCase))
                .Where(a => TITILER_VALID_TYPE.Contains(a.Value.MediaType?.MediaType))
                .OrderByDescending(a => a.Key)
                .ToDictionary(a => a.Key, a => a.Value);
            if (overviewAssets.Count == 3 && projext.IsDeclared) return overviewAssets;

            var eoStacExtension = stacItem.EoExtension();
            if (eoStacExtension.IsDeclared)
            {
                overviewAssets = stacItem.Assets.Where(a =>
                {
                    var bands = a.Value.GetProperty<EoBandObject[]>("eo:bands");
                    if (bands == null) return false;
                    return bands.Any(band => OVERVIEW_NAMES.ToList().Contains(band.CommonName.ToString()));
                })
                .Where(a => TITILER_VALID_TYPE.Contains(a.Value.MediaType?.MediaType))
                .OrderByDescending(a => a.Value.GetProperty<EoBandObject[]>("eo:bands").First().CommonName.ToString())
                .Take(3)
                .ToDictionary(a => a.Key, a => a.Value);
            }
            if (overviewAssets.Count >= 1 && projext != null) return overviewAssets;

            overviewAssets = stacItem.Assets
                .Where(a => a.Key == "overview")
                .Where(a => TITILER_VALID_TYPE.Contains(a.Value.MediaType?.MediaType))
                .ToDictionary(a => a.Key, a => a.Value);
            if (overviewAssets.Count == 1) return overviewAssets;

            overviewAssets = stacItem.Assets
                .Where(a => a.Key == "composite")
                .Where(a => TITILER_VALID_TYPE
                .Contains(a.Value.MediaType?.MediaType)).ToDictionary(a => a.Key, a => a.Value);
            if (overviewAssets.Count == 1) return overviewAssets;

            if (eoStacExtension != null)
            {
                overviewAssets = stacItem.Assets.Where(a =>
                {
                    var bands = a.Value.GetProperty<EoBandObject[]>("eo:bands");
                    if (bands == null) return false;
                    return bands.Any(band => OVERVIEW_NAMES.ToList().Contains(band.CommonName.ToString()));
                })
                .Where(a => TITILER_VALID_TYPE.Contains(a.Value.MediaType?.MediaType))
                .ToDictionary(a => a.Key, a => a.Value);
            }
            if (overviewAssets.Count >= 1) return overviewAssets;

            return new Dictionary<string, StacAsset>();
        }

        public Dictionary<string, StacAsset> SelectOverviewCombinationAssets(StacCollection stacCollection)
        {

            Dictionary<string, StacAsset> overviewAssets = stacCollection.Assets
                                                            .Where(a => a.Value.Roles != null)
                                                            .Where(a => a.Value.Roles.Contains("overview") ||
                                                                        a.Value.Roles.Contains("visual"))
                                                            .Where(a => a.Key.Contains("overview"))
                                                            .Where(a => TITILER_VALID_TYPE.Contains(a.Value.MediaType?.MediaType))
                                                            .Take(1)
                                                            .ToDictionary(a => a.Key, a => a.Value);
            if (overviewAssets.Count == 1) return overviewAssets;

            overviewAssets = stacCollection.Assets.Where(a => a.Value.Roles != null)
                                                            .Where(a => a.Value.Roles.Contains("overview") ||
                                                                        a.Value.Roles.Contains("visual"))
                                                            .OrderBy(a => a.Value.GetProperty<long>("size"))
                                                            .Where(a => TITILER_VALID_TYPE.Contains(a.Value.MediaType?.MediaType))
                                                            .Take(1)
                                                            .ToDictionary(a => a.Key, a => a.Value);
            if (overviewAssets.Count == 1) return overviewAssets;

            overviewAssets = stacCollection.Assets
                .Where(a => a.Key == "overview")
                .Where(a => TITILER_VALID_TYPE.Contains(a.Value.MediaType?.MediaType))
                .ToDictionary(a => a.Key, a => a.Value);
            if (overviewAssets.Count == 1) return overviewAssets;

            overviewAssets = stacCollection.Assets
                .Where(a => a.Key == "composite")
                .Where(a => TITILER_VALID_TYPE
                .Contains(a.Value.MediaType?.MediaType)).ToDictionary(a => a.Key, a => a.Value);
            if (overviewAssets.Count == 1) return overviewAssets;

            return new Dictionary<string, StacAsset>();
        }

        public Uri BuildServiceUri(Uri stacItemUri, IDictionary<string, StacAsset> overviewAssets)
        {
            Uri finalItemUri = Configuration.MapUri(stacItemUri);

            return new Uri(GetTilerUri(finalItemUri, overviewAssets),
                string.Format("/stac/tiles/WebMercatorQuad/{{z}}/{{x}}/{{y}}.png?url={0}&assets={1}&rescale={2}&color_formula=&resampling_method=average",
                    finalItemUri,
                    string.Join(",", overviewAssets.Keys.Take(3)),
                    string.Join(",", GetScale(overviewAssets.Values.First())),
                    GetColorFormula(overviewAssets)
                    ));
        }

        private Uri GetTilerUri(Uri finalItemUri, IDictionary<string, StacAsset> overviewAssets)
        {
            return Configuration.GetService(finalItemUri);
        }

        private static double?[] GetScale(StacAsset stacAsset)
        {
            // REMOVED statistics analysis
            // if (stacAsset.RasterExtension().Bands != null && stacAsset.RasterExtension().Bands.Count() > 0)
            // {
            //     IEnumerable<double?[]> scales = stacAsset.RasterExtension().Bands.Select(b => new double?[2] { b.Statistics?.Minimum, b.Statistics?.Minimum });
            //     if (scales.All(s => s[0].HasValue && s[1].HasValue && s[0].Value < s[1].Value))
            //     {
            //         return scales.SelectMany(s => new double?[2] { s[0].HasValue? s[0].Value : 0,
            //                                                     s[1].HasValue? s[1].Value : 10000 }).ToArray();
            //     }
            // }

            // if (stacAsset.Roles.Contains("visual") ||
            //     stacAsset.Roles.Contains("overview"))
            //     return new double?[0];
            if (stacAsset.Roles.Contains("composite"))
            {
                return new double?[2] { 0, 255 };
            }
            if (stacAsset.Roles.Contains("sigma0") ||
                stacAsset.Roles.Contains("beta0") ||
                stacAsset.Roles.Contains("gamma0")
                )
            {
                if (stacAsset.Roles.Contains("decibel"))
                {
                    return new double?[2] { -25, 5 };
                }
            }
            if (stacAsset.Roles.Contains("radiance") ||
                stacAsset.Roles.Contains("reflectance")
                )
            {
                return new double?[2] { 0, 10000 };
            }
            return new double?[2] { 0, 255 };
        }

        private object GetColorFormula(IDictionary<string, StacAsset> overviewAssets)
        {
            if (overviewAssets.Count == 1 &&
                 (overviewAssets.First().Value.Roles.Contains("visual") ||
                 overviewAssets.First().Value.Roles.Contains("overview")) &&
                 overviewAssets.First().Value.Roles.Contains("reflectance"))
                return "";

            if (overviewAssets.Count == 3 &&
                 overviewAssets.All(a => a.Value.Roles.Contains("reflectance")))
                return "Gamma RGB 1.5 Saturation 1.1 Sigmoidal RGB 15 0.35";
            return "";
        }
    }
}
