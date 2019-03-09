using System;
using System.Collections.Generic;
using Frontenac.Blueprints;
using Frontenac.Blueprints.Contracts;
using Frontenac.Blueprints.Util;

namespace Frontenac.Redis
{
    public abstract class RedisElement : DictionaryElement
    {
        internal readonly long RawId;
        protected readonly RedisGraph RedisGraph;

        protected RedisElement(long id, RedisGraph graph)
            : base(graph)
        {
            if (graph == null)
                throw new ArgumentNullException(nameof(graph));

            RawId = id;
            RedisGraph = graph;
        }

        public override object Id => RawId;

        public override object GetProperty(string key)
        {
            ElementContract.ValidateGetProperty(key);
            return RedisGraph.GetProperty(this, key);
        }

        public override IEnumerable<string> GetPropertyKeys()
        {
            return RedisGraph.GetPropertyKeys(this);
        }

        public override void SetProperty(string key, object value)
        {
            RedisGraph.SetProperty(this, key, value);
        }

        public override object RemoveProperty(string key)
        {
            ElementContract.ValidateRemoveProperty(key);
            return RedisGraph.RemoveProperty(this, key);
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
