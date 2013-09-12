using System;
using System.Diagnostics.Contracts;
using System.Linq;
using Frontenac.Blueprints;
using System.Collections.Generic;
using Frontenac.Blueprints.Util;
using Grave.Esent;

namespace Grave
{
    public abstract class GraveElement : IElement
    {
        protected readonly GraveGraph Graph;
        protected readonly EsentTable Table;
        protected readonly int RawId;

        protected GraveElement(GraveGraph graph, EsentTable table, int id)
        {
            Contract.Requires(graph != null);
            Contract.Requires(table != null);

            Graph = graph;
            Table = table;
            RawId = id;
        }

        public object GetProperty(string key)
        {
            if(string.IsNullOrWhiteSpace(key))
                throw new ArgumentException("key");

            return Table.ReadCell(RawId, key);
        }

        public IEnumerable<string> GetPropertyKeys()
        {
            return Table.GetColumnsForRow(RawId).Where(t => !t.StartsWith("$"));
        }

        public void SetProperty(string key, object value)
        {
            if(string.IsNullOrWhiteSpace(key))
                throw new ArgumentException("key");

            Table.WriteCell(RawId, key, value);
            SetIndexedKeyValue(key, value);
        }

        public object RemoveProperty(string key)
        {
            var result = Table.DeleteCell(RawId, key);
            SetIndexedKeyValue(key, null);
            return result;
        }

        void SetIndexedKeyValue(string key, object value)
        {
            Contract.Requires(!string.IsNullOrWhiteSpace(key));

            var type = this is IVertex ? typeof(IVertex) : typeof(IEdge);
            var indices = Graph.GetIndices(type, false);
            if (indices.HasIndex(key))
            {
                var generation = indices.Set(RawId, key, key, value);
                Graph.UpdateGeneration(generation);
            }
        }

        public void Remove()
        {
            var vertex = this as IVertex;
            if (vertex != null)
                Graph.RemoveVertex(vertex);
            else
                Graph.RemoveEdge((IEdge)this);
        }

        public object Id
        {
            get { return RawId; }
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
