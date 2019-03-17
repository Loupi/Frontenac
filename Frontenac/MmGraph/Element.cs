using System.Diagnostics.Contracts;
using Frontenac.Blueprints;
using Frontenac.Blueprints.Util;

namespace MmGraph
{
    public abstract class Element : DictionaryElement
    {
        public long RawId { get; }
        protected readonly Graph RawGraph;

        protected Element(long id, Graph graph) 
            : base(graph)
        {
            Contract.Requires(graph != null);

            RawId = id;
            RawGraph = graph;
        }

        public override object Id => RawId;

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