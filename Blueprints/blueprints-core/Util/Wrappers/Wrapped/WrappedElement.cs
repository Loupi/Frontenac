using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Frontenac.Blueprints.Util.Wrappers.Wrapped
{
    public abstract class WrappedElement : Element
    {
        protected Element baseElement;

        protected WrappedElement(Element baseElement)
        {
            this.baseElement = baseElement;
        }

        public void setProperty(string key, object value)
        {
            baseElement.setProperty(key, value);
        }

        public object getProperty(string key)
        {
            return baseElement.getProperty(key);
        }

        public object removeProperty(string key)
        {
            return baseElement.removeProperty(key);
        }

        public IEnumerable<string> getPropertyKeys()
        {
            return baseElement.getPropertyKeys();
        }

        public object getId()
        {
            return baseElement.getId();
        }

        public override bool Equals(object obj)
        {
            return ElementHelper.areEqual(this, obj);
        }

        public override int GetHashCode()
        {
            return baseElement.GetHashCode();
        }

        public Element getBaseElement()
        {
            return baseElement;
        }

        public void remove()
        {
            baseElement.remove();
        }

        public override string ToString()
        {
            return baseElement.ToString();
        }
    }
}
