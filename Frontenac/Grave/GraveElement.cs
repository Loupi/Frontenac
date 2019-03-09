using System;
using System.Collections.Generic;
using Frontenac.Blueprints;
using Frontenac.Blueprints.Contracts;
using Frontenac.Blueprints.Util;

namespace Frontenac.Grave
{
    public abstract class GraveElement : DictionaryElement
    {
        protected readonly GraveGraph GraveGraph;
        internal readonly int RawId;
        
        protected GraveElement(GraveGraph graph, int id)
            : base(graph)
        {
            if (graph == null)
                throw new ArgumentNullException(nameof(graph));

            GraveGraph = graph;
            RawId = id;
        }

        public override object Id => RawId;

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
            ElementContract.ValidateSetProperty(key, value);
            GraveGraph.SetProperty(this, key, value);
        }

        public override object RemoveProperty(string key)
        {
            ElementContract.ValidateRemoveProperty(key);
            return GraveGraph.RemoveProperty(this, key);
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