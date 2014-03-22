using System.Collections.Generic;
using System.Diagnostics.Contracts;
using Frontenac.Blueprints;
using Frontenac.Blueprints.Util;
using Frontenac.Grave.Esent;

namespace Frontenac.Grave
{
    public abstract class GraveElement : DictionaryElement
    {
        protected readonly GraveGraph GraveGraph;
        internal readonly int RawId;
        internal readonly EsentTable Table;

        protected GraveElement(GraveGraph graph, EsentTable table, int id):base(graph)
        {
            Contract.Requires(graph != null);
            Contract.Requires(table != null);

            GraveGraph = graph;
            Table = table;
            RawId = id;
        }

        public override object GetProperty(string key)
        {
            return GraveGraph.GetProperty(this, key);
        }

        public override IEnumerable<string> GetPropertyKeys()
        {
            return GraveGraph.GetPropertyKeys(this);
        }

        public override void SetProperty(string key, object value)
        {
            GraveGraph.SetProperty(this, key, value);
        }

        public override object RemoveProperty(string key)
        {
            return GraveGraph.RemoveProperty(this, key);
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
            var indices = GraveGraph.GetIndices(type, false);
            if (!indices.HasIndex(key)) return;
            var generation = indices.Set(RawId, key, key, value);
            GraveGraph.UpdateGeneration(generation);
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