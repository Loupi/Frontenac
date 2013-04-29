using System;
using System.Collections.Generic;

namespace Frontenac.Blueprints.Util.Wrappers.Id
{
    public abstract class IdElement : IElement
    {
        protected readonly IElement BaseElement;
        protected readonly IdGraph IdGraph;
        protected readonly bool PropertyBased;

        protected IdElement(IElement baseElement, IdGraph idGraph, bool propertyBased)
        {
            BaseElement = baseElement;
            IdGraph = idGraph;
            PropertyBased = propertyBased;
        }

        public object GetProperty(string key)
        {
            if (PropertyBased && key == IdGraph.Id)
                return null;

            return BaseElement.GetProperty(key);
        }

        public IEnumerable<string> GetPropertyKeys()
        {
            if (PropertyBased)
            {
                IEnumerable<string> keys = BaseElement.GetPropertyKeys();
                var s = new HashSet<string>(keys);
                s.Remove(IdGraph.Id);
                return s;
            }

            return BaseElement.GetPropertyKeys();
        }

        public void SetProperty(string key, object value)
        {
            if (PropertyBased && key == IdGraph.Id)
                throw new ArgumentException(string.Concat("Unable to set value for reserved property ", IdGraph.Id));

            BaseElement.SetProperty(key, value);
        }

        public object RemoveProperty(string key)
        {
            if (PropertyBased && key == IdGraph.Id)
                throw new ArgumentException(string.Concat("Unable to remove value for reserved property ", IdGraph.Id));

            return BaseElement.RemoveProperty(key);
        }

        public object GetId()
        {
            return PropertyBased
                ? BaseElement.GetProperty(IdGraph.Id)
                : BaseElement.GetId();
        }

        public override int GetHashCode()
        {
            return BaseElement.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            return ElementHelper.AreEqual(this, obj);
        }

        public void Remove()
        {
            var vertex = this as IVertex;
            if (vertex != null)
                IdGraph.RemoveVertex(vertex);
            else
                IdGraph.RemoveEdge((IEdge)this);
        }

        public override string ToString()
        {
            return BaseElement.ToString();
        }
    }
}
