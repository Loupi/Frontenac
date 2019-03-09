using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Frontenac.Blueprints.Contracts;
using Frontenac.Blueprints.Util;

namespace Frontenac.Blueprints.Impls.TG
{
    [Serializable]
    internal abstract class TinkerElement : DictionaryElement
    {
        protected readonly TinkerGraph TinkerGraph;
        protected readonly string RawId;
        protected readonly ConcurrentDictionary<string, object> Properties = new ConcurrentDictionary<string, object>();

        protected TinkerElement(string id, TinkerGraph tinkerGraph):base(tinkerGraph)
        {
            if (id == null)
                throw new ArgumentNullException(nameof(id));
            if (tinkerGraph == null)
                throw new ArgumentNullException(nameof(tinkerGraph));
            TinkerGraph = tinkerGraph;
            RawId = id;
        }

        public override IEnumerable<string> GetPropertyKeys()
        {
            return Properties.Keys.ToArray();
        }

        public override object GetProperty(string key)
        {
            ElementContract.ValidateGetProperty(key);
            return Properties.Get(key);
        }

        public override void SetProperty(string key, object value)
        {
            this.ValidateProperty(key, value);
            var oldValue = Properties.Put(key, value);
            if (this is TinkerVertex)
                TinkerGraph.VertexKeyIndex.AutoUpdate(key, value, oldValue, this);
            else
                TinkerGraph.EdgeKeyIndex.AutoUpdate(key, value, oldValue, this);
        }

        public override object RemoveProperty(string key)
        {
            ElementContract.ValidateRemoveProperty(key);
            var oldValue = Properties.JavaRemove(key);
            if (this is TinkerVertex)
                TinkerGraph.VertexKeyIndex.AutoRemove(key, oldValue, this);
            else
                TinkerGraph.EdgeKeyIndex.AutoRemove(key, oldValue, this);

            return oldValue;
        }

        public override object Id => RawId;

        public override void Remove()
        {
            var vertex = this as IVertex;
            if (vertex != null)
                Graph.RemoveVertex(vertex);
            else
                Graph.RemoveEdge((IEdge) this);
        }

        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            return this.AreEqual(obj);
        }
    }
}