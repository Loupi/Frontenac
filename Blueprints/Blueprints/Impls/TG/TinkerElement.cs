using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using Frontenac.Blueprints.Util;

namespace Frontenac.Blueprints.Impls.TG
{
    [Serializable]
    internal abstract class TinkerElement : DictionaryElement
    {
        protected readonly TinkerGraĥ TinkerGraĥ;
        protected readonly string RawId;
        protected ConcurrentDictionary<string, object> Properties = new ConcurrentDictionary<string, object>();

        protected TinkerElement(string id, TinkerGraĥ tinkerGraĥ):base(tinkerGraĥ)
        {
            Contract.Requires(id != null);
            Contract.Requires(tinkerGraĥ != null);
            TinkerGraĥ = tinkerGraĥ;
            RawId = id;
        }

        public override IEnumerable<string> GetPropertyKeys()
        {
            return Properties.Keys.ToArray();
        }

        public override object GetProperty(string key)
        {
            return Properties.Get(key);
        }

        public override void SetProperty(string key, object value)
        {
            this.ValidateProperty(key, value);
            var oldValue = Properties.Put(key, value);
            if (this is TinkerVertex)
                TinkerGraĥ.VertexKeyIndex.AutoUpdate(key, value, oldValue, this);
            else
                TinkerGraĥ.EdgeKeyIndex.AutoUpdate(key, value, oldValue, this);
        }

        public override object RemoveProperty(string key)
        {
            var oldValue = Properties.JavaRemove(key);
            if (this is TinkerVertex)
                TinkerGraĥ.VertexKeyIndex.AutoRemove(key, oldValue, this);
            else
                TinkerGraĥ.EdgeKeyIndex.AutoRemove(key, oldValue, this);

            return oldValue;
        }

        public override object Id
        {
            get { return RawId; }
        }

        public override void Remove()
        {
            var vertex = this as IVertex;
            if (vertex != null)
                base.Graph.RemoveVertex(vertex);
            else
                base.Graph.RemoveEdge((IEdge) this);
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