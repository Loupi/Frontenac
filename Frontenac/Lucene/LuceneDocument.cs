using System;
using System.Runtime.Serialization;
using Frontenac.Blueprints.Geo;
using Frontenac.Infrastructure;
using Frontenac.Infrastructure.Indexing;
using Lucene.Net.Documents;
using Lucene.Net.Spatial.Prefix;
using Lucene.Net.Spatial.Prefix.Tree;
using Spatial4n.Core.Context;

namespace Frontenac.Lucene
{
    public class LuceneDocument : IDocument
    {
        private const string NullColumnName = "$gn";

        private readonly Document _document;
        private readonly ObjectIDGenerator _idGenerator = new ObjectIDGenerator();

        public LuceneDocument(Document document)
        {
            if (document == null)
                throw new ArgumentNullException(nameof(document));

            _document = document;
        }

        public bool Write(string key, object value)
        {
            var result = true;

            if (value == null)
            {
                _document.Add(new Field(NullColumnName, key, Field.Store.NO, Field.Index.NOT_ANALYZED));
            }
            else if (value is string)
            {
                string sval = value.ToString();
                _document.Add(new Field(key, sval, Field.Store.NO, Field.Index.ANALYZED_NO_NORMS));
            }
            else if (value is sbyte || value is byte || value is short || value is ushort || value is int ||
                     value is uint/*)
            {
                var val = Convert.ToInt32(value);
                _document.Add(new NumericField(key).SetLongValue(val));
            }
            else if (*/  || value is long || value is ulong)
            {
                //var val = Convert.ToInt64(value);
                long lval = value.ToInt64();
                _document.Add(new NumericField(key).SetLongValue(lval));
            }
            else if (value is float)
            {
                var val = Convert.ToSingle(value);
                _document.Add(new NumericField(key).SetFloatValue(val));
            }
            else if (value is double)
            {
                double dval = Convert.ToDouble(value);
                _document.Add(new NumericField(key).SetDoubleValue(dval));
            }
            else if (value is GeoPoint)
            {
                var grid = new GeohashPrefixTree(SpatialContext.GEO, 11);
                var strategy = new RecursivePrefixTreeStrategy(grid, key);
                var geoCoordinate = value as GeoPoint;
                var shape = SpatialContext.GEO.MakePoint(geoCoordinate.Latitude, geoCoordinate.Longitude);
                var fields = strategy.CreateIndexableFields(shape);
                foreach (var field in fields)
                {
                    _document.Add(field);
                }
            }
            else
            {
                result = false;
            }

            return result;
        }

        public bool Present(object value)
        {
            bool firstTime;
            _idGenerator.GetId(value, out firstTime);
            return !firstTime;
        }
    }
}