using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using Frontenac.Blueprints;
using Frontenac.Blueprints.Util;
using Grave.Esent;

namespace Grave
{
    public abstract class GraveElement : DictionaryElement
    {
        protected readonly GraveGraph Graph;
        protected readonly int RawId;
        protected readonly EsentTable Table;

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
            return Table.ReadCell(RawId, key);
        }

        public override IEnumerable<string> GetPropertyKeys()
        {
            return Table.GetColumnsForRow(RawId).Where(t => !t.StartsWith("$"));
        }

        public override void SetProperty(string key, object value)
        {
            Table.WriteCell(RawId, key, value);
            SetIndexedKeyValue(key, value);
        }

        public override object RemoveProperty(string key)
        {
            var result = Table.DeleteCell(RawId, key);
            SetIndexedKeyValue(key, null);
            return result;
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

        private void SetIndexedKeyValue(string key, object value)
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
            return ElementHelper.AreEqual(this, obj);
        }
    }
}