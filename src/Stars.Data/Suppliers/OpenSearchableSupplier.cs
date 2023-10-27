using Microsoft.Extensions.Logging;
using System;
using System.Collections.Specialized;
using System.Linq;
using System.Threading.Tasks;
using Terradue.Stars.Data.Routers;
using Terradue.OpenSearch;
using Terradue.OpenSearch.Engine;
using Terradue.OpenSearch.Result;
using Terradue.Stars.Services.Translator;
using Terradue.Stars.Services.Model.Stac;
using Terradue.Stars.Interface.Supplier;
using Terradue.Stars.Interface;
using System.Text.RegularExpressions;
using Stac;
using System.Threading;
using Stac.Api.Interfaces;
using Stac.Api.Models.Cql2;
using Microsoft.AspNetCore.Mvc;
using Itenso.TimePeriod;
using Terradue.GeoJson.Geometry;
using Terradue.Stars.Geometry.Wkt;

namespace Terradue.Stars.Data.Suppliers
{
    public class OpenSearchableSupplier : ISupplier
    {
        protected IOpenSearchable openSearchable;
        protected TranslatorManager translatorManager;
        protected ILogger logger;
        protected OpenSearchEngine opensearchEngine;

        public OpenSearchableSupplier(ILogger<OpenSearchableSupplier> logger, TranslatorManager translatorManager) : this(logger as ILogger, translatorManager)
        { }

        internal OpenSearchableSupplier(ILogger logger, TranslatorManager translatorManager)
        {
            this.opensearchEngine = new OpenSearchEngine();
            this.opensearchEngine.RegisterExtension(new Terradue.OpenSearch.Engine.Extensions.AtomOpenSearchEngineExtension());
            this.opensearchEngine.RegisterExtension(new Terradue.OpenSearch.GeoJson.Extensions.FeatureCollectionOpenSearchEngineExtension());
            this.logger = logger;
            this.translatorManager = translatorManager;
        }

        public int Priority { get; set; }
        public string Key { get; set; }


        public virtual string Id => "OS" + nameof(openSearchable);

        public virtual string Label => "Generic data supplier for OpenSearch interfaces";

        public virtual async Task<IResource> SearchForAsync(IResource node, CancellationToken ct, string identifierRegex = null)
        {
            return (IResource)new OpenSearchResultItemRoutable((await QueryAsync(node, ct, identifierRegex)).Items.FirstOrDefault(), new Uri("os://" + openSearchable.Identifier), logger);
        }

        public virtual async Task<IOpenSearchResultCollection> QueryAsync(IResource node, CancellationToken ct, string identifierRegex = null)
        {
            // TEMP skipping catalog for the moment
            if (!(node is IItem)) return null;

            // Let's translate the node to STAC
            var stacNode = await translatorManager.TranslateAsync<StacNode>(node, ct);

            IOpenSearchResultCollection results = null;

            if (stacNode == null)
            {
                results = await OpenSearchQuery(node, identifierRegex);
            }
            else
            {
                results = await OpenSearchQuery(stacNode, identifierRegex);
            }

            if (results == null || results.Count == 0)
                return null;
            // TODO manage more than first item
            return results;
        }

        protected virtual async Task<IOpenSearchResultCollection> OpenSearchQuery(IResource node, string identifierRegex = null)
        {
            if (!(node is IItem)) return null;
            NameValueCollection nvc = CreateOpenSearchParametersFromItem(node as IItem, identifierRegex);
            if (nvc == null) return null;

            return await Task.Run<AtomFeed>(() => (AtomFeed)opensearchEngine.Query(openSearchable, nvc, typeof(AtomFeed)));
        }

        private NameValueCollection CreateOpenSearchParametersFromItem(IItem item, string identifierRegex = null)
        {
            string identifier = item.Id;
            NameValueCollection parameters = new NameValueCollection();
            if (identifierRegex != null)
            {
                Regex regex = new Regex(identifierRegex);
                var match = regex.Match(identifier);
                if (match.Success)
                {
                    GroupCollection groups = match.Groups;
                    foreach (string groupName in regex.GetGroupNames())
                    {
                        switch (groupName)
                        {
                            case "id":
                            case "uid":
                                parameters.Set("{http://a9.com/-/opensearch/extensions/geo/1.0/}uid", groups[groupName].Value);
                                break;
                            default:
                                parameters.Set(groupName, groups[groupName].Value);
                                break;
                        }
                    }
                    return parameters;
                }
            }

            parameters.Set("{http://a9.com/-/opensearch/extensions/geo/1.0/}uid", identifier);

            // USGS: Setting the collection to parentIdentifier parameter
            if (item is StacItemNode)
            {
                var stacItem = item as StacItemNode;
                if (stacItem.StacItem.StacExtensions.Any(e => e.Contains("landsat")) && stacItem.StacItem.GetProperty<string>("landsat:collection_category") != null)
                {
                    if (stacItem.StacItem.GetProperty<string>("landsat:collection_category").StartsWith("T")
                        && stacItem.StacItem.GetProperty<string>("landsat:collection_category") != "C2")
                    {
                        if (stacItem.StacItem.GetProperty<string>("landsat:correction").StartsWith("L1"))
                            parameters.Set("{http://a9.com/-/opensearch/extensions/eo/1.0/}parentIdentifier", "landsat_ot_c2_l1");
                        if (stacItem.StacItem.GetProperty<string>("landsat:correction").StartsWith("L2"))
                            parameters.Set("{http://a9.com/-/opensearch/extensions/eo/1.0/}parentIdentifier", "landsat_ot_c2_l2");
                    }
                }
            }

            return parameters;
        }

        public virtual Task<IOrder> Order(IOrderable orderableRoute)
        {
            throw new NotSupportedException();
        }

        public async Task<IResource> SearchForAsync(ISearchExpression searchExpression, CancellationToken ct)
        {
            NameValueCollection nvc = CreateOpenSearchParametersFromExpression(searchExpression);
            if (nvc == null) return null;

            AtomFeed atomFeed = await Task.Run<AtomFeed>(() => (AtomFeed)opensearchEngine.Query(openSearchable, nvc, typeof(AtomFeed)));

            return new OpenSearchResultFeedRoutable(atomFeed, new Uri("os://" + openSearchable.Identifier), logger);
        }

        private NameValueCollection CreateOpenSearchParametersFromExpression(ISearchExpression searchExpression)
        {
            // Here stands the big piece of work to translate the search expression to OpenSearch parameters

            // First, we only support CQL2SearchExpression for now
            if (!(searchExpression is CQL2Expression))
            {
                logger.LogWarning("The OpenSearchableSupplier supplier cannot search for resource from a search expression other than CQL2");
                return null;
            }

            // Basically, Opensearch shall supports only the first level of search expression
            CQL2Expression cql2Expression = searchExpression as CQL2Expression;

            // Prepare an empty collection of parameters
            NameValueCollection parameters = new NameValueCollection();
            int level = 0;

            // Let's iterate over the search expression
            FillParametersFromBooleanExpression(cql2Expression.Expression, parameters, level);

            return parameters;

        }

        private void FillParametersFromBooleanExpression(BooleanExpression booleanExpression, NameValueCollection parameters, int level)
        {
            if (level > 1)
                throw new NotSupportedException("The OpenSearchableSupplier supplier cannot search for resource from a search expression with nested boolean expression");
            switch (booleanExpression)
            {
                case AndOrExpression andOrExpression:
                    // we do not support OR operator
                    if (andOrExpression.Op == AndOrExpressionOp.Or)
                        throw new NotSupportedException("The OpenSearchableSupplier supplier cannot search for resource from a search expression with OR operator");
                    // we simply recurse on the two operands
                    FillParametersFromBooleanExpression(andOrExpression.Args[0], parameters, level + 1);
                    FillParametersFromBooleanExpression(andOrExpression.Args[1], parameters, level + 1);
                    return;
                case NotExpression notExpression:
                    // we do not support NOT operator
                    throw new NotSupportedException("The OpenSearchableSupplier supplier cannot search for resource from a search expression with NOT operator");
                case SpatialPredicate spatialPredicate:
                    FillParametersFromSpatialPredicate(spatialPredicate, parameters, level + 1);
                    return;
                case TemporalPredicate temporalPredicate:
                    FillParametersFromTemporalPredicate(temporalPredicate, parameters, level + 1);
                    return;
                case ComparisonPredicate comparisonPredicate:
                    FillParametersFromComparisonPredicate(comparisonPredicate, parameters, level + 1);
                    return;
                case ArrayPredicate arrayPredicate:
                    FillParametersFromArrayPredicate(arrayPredicate, parameters, level + 1);
                    return;
            }
        }

        private void FillParametersFromArrayPredicate(ArrayPredicate arrayPredicate, NameValueCollection parameters, int v)
        {
            throw new NotImplementedException();
        }

        private void FillParametersFromTemporalPredicate(TemporalPredicate temporalPredicate, NameValueCollection parameters, int v)
        {
            // We support only the comparison between a property and a literal
            // So we extract the property and the literal
            string propertyName = null;
            Itenso.TimePeriod.ITimeInterval timeInterval = null;
            foreach (var arg in temporalPredicate.Args)
            {
                if (arg is PropertyRef propertyRef)
                {
                    propertyName = propertyRef.Property;
                }
                else if (arg is ITemporalLiteral temporalLiteral)
                {
                    timeInterval = TemporalLiteralToTimeInterval(temporalLiteral);
                }
            }
            if (propertyName == null || timeInterval == null)
                throw new NotSupportedException("The OpenSearchableSupplier supplier cannot search for resource from a search expression with temporal predicate with other than property and literal");
            // check the property name
            switch (propertyName)
            {
                case "datetime":
                    FillStartStopParametersFromTimeInterval(timeInterval, temporalPredicate.Op, parameters);
                    return;
                case "updated":
                    // only works with the after operator
                    if (temporalPredicate.Op != TemporalPredicateOp.T_after)
                        throw new NotSupportedException($"The OpenSearchableSupplier supplier cannot search for resource from a search expression with temporal predicate on '{propertyName}' property with other than after operator");
                    parameters.Set("{http://purl.org/dc/terms/}modified", timeInterval.Start.ToString("yyyy-MM-ddTHH:mm:ssZ"));
                    return;
                default:
                    throw new NotSupportedException($"The OpenSearchableSupplier supplier cannot search for resource from a search expression with temporal predicate on '{propertyName}' property");
            }
        }

        private void FillStartStopParametersFromTimeInterval(ITimeInterval timeInterval, TemporalPredicateOp op, NameValueCollection parameters)
        {
            switch (op)
            {
                case TemporalPredicateOp.T_after:
                    parameters.Set("time:start", timeInterval.Start.ToString("yyyy-MM-ddTHH:mm:ssZ"));
                    return;
                case TemporalPredicateOp.T_before:
                    parameters.Set("time:end", timeInterval.End.ToString("yyyy-MM-ddTHH:mm:ssZ"));
                    return;
                case TemporalPredicateOp.T_during:
                    parameters.Set("time:start", timeInterval.Start.ToString("yyyy-MM-ddTHH:mm:ssZ"));
                    parameters.Set("time:end", timeInterval.End.ToString("yyyy-MM-ddTHH:mm:ssZ"));
                    parameters.Set("time:relation", "during");
                    return;
                case TemporalPredicateOp.T_equals:
                    parameters.Set("time:start", timeInterval.Start.ToString("yyyy-MM-ddTHH:mm:ssZ"));
                    parameters.Set("time:end", timeInterval.End.ToString("yyyy-MM-ddTHH:mm:ssZ"));
                    parameters.Set("time:relation", "equals");
                    return;
                case TemporalPredicateOp.T_disjoint:
                    parameters.Set("time:start", timeInterval.Start.ToString("yyyy-MM-ddTHH:mm:ssZ"));
                    parameters.Set("time:end", timeInterval.End.ToString("yyyy-MM-ddTHH:mm:ssZ"));
                    parameters.Set("time:relation", "disjoint");
                    return;
                case TemporalPredicateOp.T_contains:
                    parameters.Set("time:start", timeInterval.Start.ToString("yyyy-MM-ddTHH:mm:ssZ"));
                    parameters.Set("time:end", timeInterval.End.ToString("yyyy-MM-ddTHH:mm:ssZ"));
                    parameters.Set("time:relation", "contains");
                    return;
                default:
                    parameters.Set("time:start", timeInterval.Start.ToString("yyyy-MM-ddTHH:mm:ssZ"));
                    parameters.Set("time:end", timeInterval.End.ToString("yyyy-MM-ddTHH:mm:ssZ"));
                    parameters.Set("time:relation", "intersects");
                    return;
            }
        }

        private ITimeInterval TemporalLiteralToTimeInterval(ITemporalLiteral temporalLiteral)
        {
            if (temporalLiteral is InstantLiteral timeInstantLiteral)
            {
                return new TimeInterval(timeInstantLiteral.DateTime.DateTime, timeInstantLiteral.DateTime.DateTime);
            }
            else if (temporalLiteral is IntervalLiteral timePeriodLiteral)
            {
                return timePeriodLiteral.TimeInterval;
            }
            else
            {
                throw new NotSupportedException("The OpenSearchableSupplier supplier cannot search for resource from a search expression with temporal literal other than instant or period");
            }
        }

        private void FillParametersFromSpatialPredicate(SpatialPredicate spatialPredicate, NameValueCollection parameters, int v)
        {
            // We support only the comparison between a property and a literal
            // So we extract the property and the literal
            string propertyName = null;
            ISpatialLiteral spatialLiteral = null;
            foreach (var arg in spatialPredicate.Args)
            {
                if (arg is PropertyRef propertyRef)
                {
                    propertyName = propertyRef.Property;
                }
                else if (arg is ISpatialLiteral spatialLiteralArg)
                {
                    spatialLiteral = spatialLiteralArg;
                }
            }
            if (propertyName == null || spatialLiteral == null)
                throw new NotSupportedException("The OpenSearchableSupplier supplier cannot search for resource from a search expression with spatial predicate with other than property and literal");
            // check the property name
            switch (propertyName)
            {
                case "geometry":
                    FillParametersFromGeometryLiteral(spatialLiteral, spatialPredicate.Op, parameters);
                    return;
                default:
                    throw new NotSupportedException($"The OpenSearchableSupplier supplier cannot search for resource from a search expression with spatial predicate on '{propertyName}' property");
            }
        }

        private void FillParametersFromGeometryLiteral(ISpatialLiteral spatialLiteral, SpatialPredicateOp op, NameValueCollection parameters)
        {
            switch (spatialLiteral)
            {
                case GeometryLiteral geometryLiteral:
                    switch (op)
                    {
                        case SpatialPredicateOp.S_intersects:
                            parameters.Set("geo:geometry", geometryLiteral.GeometryObject.ToWkt());
                            parameters.Set("geo:relation", "intersects");
                            return;
                        case SpatialPredicateOp.S_contains:
                            parameters.Set("geo:geometry", geometryLiteral.GeometryObject.ToWkt());
                            parameters.Set("geo:relation", "contains");
                            return;
                        case SpatialPredicateOp.S_disjoint:
                            parameters.Set("geo:geometry", geometryLiteral.GeometryObject.ToWkt());
                            parameters.Set("geo:relation", "disjoint");
                            return;
                        default:
                            throw new NotSupportedException($"The OpenSearchableSupplier supplier cannot search for resource from a search expression with spatial predicate with '{op}' operator");
                    }
                case EnvelopeLiteral envelopeLiteral:
                    switch (op)
                    {
                        case SpatialPredicateOp.S_intersects:
                            parameters.Set("geo:box", envelopeLiteral.ToString());
                            parameters.Set("geo:relation", "intersects");
                            return;
                        case SpatialPredicateOp.S_contains:
                            parameters.Set("geo:box", envelopeLiteral.ToString());
                            parameters.Set("geo:relation", "contains");
                            return;
                        case SpatialPredicateOp.S_disjoint:
                            parameters.Set("geo:box", envelopeLiteral.ToString());
                            parameters.Set("geo:relation", "disjoint");
                            return;
                        default:
                            throw new NotSupportedException($"The OpenSearchableSupplier supplier cannot search for resource from a search expression with spatial predicate with '{op}' operator");
                    }
                default:
                    throw new NotSupportedException($"The OpenSearchableSupplier supplier cannot search for resource from a search expression with spatial predicate with other than geometry literal");
            }
        }

        private void FillParametersFromComparisonPredicate(ComparisonPredicate comparisonPredicate, NameValueCollection parameters, int v)
        {
            switch (comparisonPredicate)
            {
                case BinaryComparisonPredicate binaryComparisonPredicate:
                    FillParametersFromBinaryComparisonPredicate(binaryComparisonPredicate, parameters, v);
                    return;
                case IsLikePredicate isLikePredicate:
                    FillParametersFromIsLikePredicate(isLikePredicate, parameters, v);
                    return;
                case IsInListPredicate isInListPredicate:
                    FillParametersFromIsInListPredicate(isInListPredicate, parameters, v);
                    return;
                case IsBetweenPredicate isBetweenPredicate:
                    FillParametersFromIsBetweenPredicate(isBetweenPredicate, parameters, v);
                    return;
                default:
                    throw new NotSupportedException($"The OpenSearchableSupplier supplier cannot search for resource from a search expression with comparison predicate with other than binary comparison, is like or is in list");
            }
        }

        private void FillParametersFromIsBetweenPredicate(IsBetweenPredicate isBetweenPredicate, NameValueCollection parameters, int v)
        {
            // We support only the comparison between a property and a literal
            // So we extract the property and the literal
            string propertyName = null;
            string min = null;
            string max = null;
            foreach (var arg in isBetweenPredicate.Args)
            {
                if (arg is PropertyRef propertyRef)
                {
                    propertyName = propertyRef.Property;
                }
                else if (arg is IScalarExpression scalarExpression)
                {
                    if (min == null)
                        min = scalarExpression.ToString();
                    else
                        max = scalarExpression.ToString();
                }
            }
            if (propertyName == null || min == null || max == null)
                throw new NotSupportedException("The OpenSearchableSupplier supplier cannot search for resource from a search expression with is between predicate with other than property and literal");
            // check the property name
            switch (propertyName)
            {
                case "collection":
                    parameters.Set("{http://a9.com/-/opensearch/extensions/eo/1.0/}parentIdentifier", $"[{min},{max}]");
                    return;
                case "keywords":
                    parameters.Set("{http://purl.org/dc/terms/}subject", $"[{min},{max}]");
                    return;
                case "eo:cloud_cover":
                    parameters.Set("{http://a9.com/-/opensearch/extensions/eo/1.0/}cloudCover", $"[{min},{max}]");
                    return;
                default:
                    throw new NotSupportedException($"The OpenSearchableSupplier supplier cannot search for resource from a search expression with is in list predicate on '{propertyName}' property");
            }
        }

        private void FillParametersFromIsInListPredicate(IsInListPredicate isInListPredicate, NameValueCollection parameters, int v)
        {
            // We support only the comparison between a property and a literal
            // So we extract the property and the literal
            string propertyName = null;
            string[] values = null;
            foreach (var arg in isInListPredicate.Args)
            {
                if (arg is PropertyRef propertyRef)
                {
                    propertyName = propertyRef.Property;
                }
                else if (arg is ScalarExpressionCollection scalarExpressions)
                {
                    values = scalarExpressions.Select(se => se.ToString()).ToArray();
                }
            }
            if (propertyName == null || values == null)
                throw new NotSupportedException("The OpenSearchableSupplier supplier cannot search for resource from a search expression with is in list predicate with other than property and literal");
            // check the property name
            switch (propertyName)
            {
                case "collection":
                    parameters.Set("{http://a9.com/-/opensearch/extensions/eo/1.0/}parentIdentifier", "{" + string.Join(",", values) + "}");
                    return;
                case "keywords":
                    parameters.Set("{http://purl.org/dc/terms/}subject", "{" + string.Join(",", values) + "}");
                    return;
                default:
                    throw new NotSupportedException($"The OpenSearchableSupplier supplier cannot search for resource from a search expression with is in list predicate on '{propertyName}' property");
            }
        }

        private void FillParametersFromIsLikePredicate(IsLikePredicate isLikePredicate, NameValueCollection parameters, int v)
        {
            // We support only the comparison between a property and a literal
            // So we extract the property and the literal
            string propertyName = null;
            string value = null;
            foreach (var arg in isLikePredicate.Args)
            {
                if (arg is PropertyRef propertyRef)
                {
                    propertyName = propertyRef.Property;
                }
                else if (arg is IScalarExpression scalarExpression)
                {
                    value = scalarExpression.ToString();
                }
            }
            if (propertyName == null || value == null)
                throw new NotSupportedException("The OpenSearchableSupplier supplier cannot search for resource from a search expression with is like predicate with other than property and literal");
            // check the property name
            switch (propertyName)
            {
                case "id":
                    parameters.Set("{http://a9.com/-/opensearch/extensions/geo/1.0/}uid", value);
                    return;
                case "collection":
                    parameters.Set("{http://a9.com/-/opensearch/extensions/eo/1.0/}parentIdentifier", value);
                    return;
                case "keywords":
                    parameters.Set("{http://purl.org/dc/terms/}subject", value);
                    return;
                case "eo:cloud_cover":
                    parameters.Set("{http://a9.com/-/opensearch/extensions/eo/1.0/}cloudCover", value);
                    return;
                default:
                    throw new NotSupportedException($"The OpenSearchableSupplier supplier cannot search for resource from a search expression with is like predicate on '{propertyName}' property");
            }
        }

        private void FillParametersFromBinaryComparisonPredicate(BinaryComparisonPredicate binaryComparisonPredicate, NameValueCollection parameters, int v)
        {
            // We support only the comparison between a property and a literal
            // So we extract the property and the literal
            string propertyName = null;
            string value = null;
            foreach (var arg in binaryComparisonPredicate.Args)
            {
                if (arg is PropertyRef propertyRef)
                {
                    propertyName = propertyRef.Property;
                }
                else if (arg is IScalarExpression scalarExpression)
                {
                    value = scalarExpression.ToString();
                }
            }
            if (propertyName == null || value == null)
                throw new NotSupportedException("The OpenSearchableSupplier supplier cannot search for resource from a search expression with comparison predicate with other than property and literal");
            // check the property name
            switch (propertyName)
            {
                case "id":
                    parameters.Set("{http://a9.com/-/opensearch/extensions/geo/1.0/}uid", ValueToNumberSetOrInterval(value, binaryComparisonPredicate.Op));
                    return;
                case "collection":
                    parameters.Set("{http://a9.com/-/opensearch/extensions/eo/1.0/}parentIdentifier", ValueToNumberSetOrInterval(value, binaryComparisonPredicate.Op));
                    return;
                case "eo:cloud_cover":
                    parameters.Set("{http://a9.com/-/opensearch/extensions/eo/1.0/}cloudCover", ValueToNumberSetOrInterval(value, binaryComparisonPredicate.Op));
                    return;
                default:
                    throw new NotSupportedException($"The OpenSearchableSupplier supplier cannot search for resource from a search expression with is like predicate on '{propertyName}' property");
            }
        }

        private string ValueToNumberSetOrInterval(string value, ComparisonPredicateOp op)
        {
            switch (op)
            {
                case ComparisonPredicateOp.Eq:
                    return value;
                case ComparisonPredicateOp.Gt:
                    return "]" + value;
                case ComparisonPredicateOp.Ge:
                    return "[" + value;
                case ComparisonPredicateOp.Lt:
                    return value + "[";
                case ComparisonPredicateOp.Le:
                    return value + "]";
                default:
                    throw new NotSupportedException($"The OpenSearchableSupplier supplier cannot search for resource from a search expression with comparison predicate with '{op}' operator");
            }
        }

        public Task<object> InternalSearchExpressionAsync(ISearchExpression searchExpression, CancellationToken ct)
        {
            NameValueCollection nvc = CreateOpenSearchParametersFromExpression(searchExpression);
            if (nvc == null) return null;
            // return a dictionary
            return Task.FromResult<object>(nvc.AllKeys.ToDictionary(k => k, k => nvc[k]));
        }
    }
}