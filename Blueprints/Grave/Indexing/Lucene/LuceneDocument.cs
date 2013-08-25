using System;
using System.Runtime.Serialization;
using Lucene.Net.Documents;

namespace Grave.Indexing.Lucene
{
    public class LuceneDocument : IDocument
    {
        const string NullColumnName = "$gn";

        readonly Document _document;
        readonly ObjectIDGenerator _idGenerator = new ObjectIDGenerator();

        public LuceneDocument(Document document)
        {
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
                var val = value.ToString();
                _document.Add(new Field(key, val, Field.Store.NO, Field.Index.ANALYZED));
            }
            else if (value is sbyte || value is byte || value is short || value is ushort || value is int || value is uint)
            {
                var val = Convert.ToInt32(value);
                _document.Add(new NumericField(key).SetIntValue(val));
            }
            else if (value is long || value is ulong)
            {
                var val = Convert.ToInt64(value);
                _document.Add(new NumericField(key).SetLongValue(val));
            }
            else if (value is float)
            {
                var val = Convert.ToSingle(value);
                _document.Add(new NumericField(key).SetFloatValue(val));
            }
            else if (value is double)
            {
                var val = Convert.ToDouble(value);
                _document.Add(new NumericField(key).SetDoubleValue(val));
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
