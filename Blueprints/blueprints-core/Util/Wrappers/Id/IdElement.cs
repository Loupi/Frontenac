using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Frontenac.Blueprints.Util.Wrappers.Id
{
    public abstract class IdElement : Element
    {
        protected readonly Element baseElement;
        protected readonly IdGraph idGraph;
        protected readonly bool propertyBased;

        protected IdElement(Element baseElement, IdGraph idGraph, bool propertyBased)
        {
            this.baseElement = baseElement;
            this.idGraph = idGraph;
            this.propertyBased = propertyBased;
        }

        public object getProperty(string key)
        {
            if (propertyBased && key == IdGraph.ID)
                return null;

            return baseElement.getProperty(key);
        }

        public IEnumerable<string> getPropertyKeys()
        {
            if (propertyBased)
            {
                IEnumerable<string> keys = baseElement.getPropertyKeys();
                HashSet<string> s = new HashSet<string>(keys);
                s.Remove(IdGraph.ID);
                return s;
            }

            return baseElement.getPropertyKeys();
        }

        public void setProperty(string key, object value)
        {
            if (propertyBased && key == IdGraph.ID)
                throw new ArgumentException(string.Concat("Unable to set value for reserved property ", IdGraph.ID));

            baseElement.setProperty(key, value);
        }

        public object removeProperty(string key)
        {
            if (propertyBased && key == IdGraph.ID)
                throw new ArgumentException(string.Concat("Unable to remove value for reserved property ", IdGraph.ID));

            return baseElement.removeProperty(key);
        }

        public object getId()
        {
            return propertyBased
                ? baseElement.getProperty(IdGraph.ID)
                : baseElement.getId();
        }

        public override int GetHashCode()
        {
            return baseElement.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            return ElementHelper.areEqual(this, obj);
        }

        public void remove()
        {
            if (this is Vertex)
                idGraph.removeVertex((Vertex)this);
            else
                idGraph.removeEdge((Edge)this);
        }

        public override string ToString()
        {
            return baseElement.ToString();
        }
    }
}
