using System.Collections.Generic;
using System.Diagnostics.Contracts;
using Frontenac.Blueprints;
using Frontenac.Blueprints.Util;
using Frontenac.Grave.Esent;

namespace Frontenac.Grave
{
    public abstract class GraveElement : DictionaryElement
    {
        protected readonly GraveGraph Graph;
        internal readonly int RawId;
        internal readonly EsentTable Table;

        protected GraveElement(GraveGraph graph, EsentTable table, int id)
        {
            Contract.Requires(graph != null);
            Contract.Requires(table != null);

            Graph = graph;
            Table = table;
            RawId = id;
        }

        public override object GetProperty(string key)
        {
            return Graph.GetProperty(this, key);
        }

        public override IEnumerable<string> GetPropertyKeys()
        {
            return Graph.GetPropertyKeys(this);
        }

        public override void SetProperty(string key, object value)
        {
            Graph.SetProperty(this, key, value);
        }

        public override object RemoveProperty(string key)
        {
            return Graph.RemoveProperty(this, key);
        }

        public override void Remove()
        {
            var vertex = this as IVertex;
            if (vertex != null)
                Graph.RemoveVertex(vertex);
            else
                Graph.RemoveEdge((IEdge) this);
        }

        public override object Id
        {
            get { return RawId; }
        }

        internal void SetIndexedKeyValue(string key, object value)
        {
            Contract.Requires(!string.IsNullOrWhiteSpace(key));

            var type = this is IVertex ? typeof (IVertex) : typeof (IEdge);
            var indices = Graph.GetIndices(type, false);
            if (indices.HasIndex(key))
            {
                var generation = indices.Set(RawId, key, key, value);
                Graph.UpdateGeneration(generation);
            }
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