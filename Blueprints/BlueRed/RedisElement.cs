using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Frontenac.Blueprints;
using Frontenac.Blueprints.Util;
using StackExchange.Redis;

namespace Frontenac.BlueRed
{
    public abstract class RedisElement : DictionaryElement
    {
        internal readonly long RawId;
        internal readonly RedisGraph _graph;

        protected RedisElement(long id, RedisGraph graph)
            : base(graph)
        {
            RawId = id;
            _graph = graph;
        }

        public override object Id
        {
            get { return RawId; }
        }

        public override object GetProperty(string key)
        {
            var db = _graph.Multiplexer.GetDatabase();
            var val = db.HashGet(GetIdentifier("properties"), key);
            return val != RedisValue.Null ? _graph.Serializer.Deserialize(val) : null;
        }

        public string GetIdentifier(string suffix)
        {
            var prefix = this is RedisVertex ? "vertex:" : "edge:";
            var identifier = string.Concat(prefix, RawId);
            if (suffix != null)
                identifier = string.Concat(identifier, ":", suffix);
            return identifier;
        }

        public string GetLabeledIdentifier(string suffix, string label)
        {
            return string.Concat(GetIdentifier(suffix), ":", label);
        }

        public override IEnumerable<string> GetPropertyKeys()
        {
            var db = _graph.Multiplexer.GetDatabase();
            var keys = db.HashKeys(GetIdentifier("properties"));
            return keys.Select((value => value.ToString())).ToArray();
        }

        public override void SetProperty(string key, object value)
        {
            var raw = _graph.Serializer.Serialize(value);
            var db = _graph.Multiplexer.GetDatabase();
            db.HashSet(GetIdentifier("properties"), key, raw);
            _graph.SetIndexedKeyValue(this, key, value);
        }

        public override object RemoveProperty(string key)
        {
            var result = GetProperty(key);
            var db = _graph.Multiplexer.GetDatabase();
            db.HashDelete(GetIdentifier("properties"), key);
            _graph.SetIndexedKeyValue(this, key, null);
            return result;
        }

        public override int GetHashCode()
        {
            return RawId.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            return this.AreEqual(obj);
        }
    }
}
