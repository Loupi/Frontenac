using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Frontenac.Blueprints.Util.Wrappers.ReadOnly
{
    public abstract class ReadOnlyElement : Element
    {
        protected Element baseElement;

        protected ReadOnlyElement(Element baseElement)
        {
            this.baseElement = baseElement;
        }

        public IEnumerable<string> getPropertyKeys()
        {
            return baseElement.getPropertyKeys();
        }

        public object getId()
        {
            return baseElement.getId();
        }

        public object removeProperty(string key)
        {
            throw new InvalidOperationException(ReadOnlyTokens.MUTATE_ERROR_MESSAGE);
        }

        public object getProperty(string key)
        {
            return baseElement.getProperty(key);
        }

        public void setProperty(string key, object value)
        {
            throw new InvalidOperationException(ReadOnlyTokens.MUTATE_ERROR_MESSAGE);
        }

        public override string ToString()
        {
            return baseElement.ToString();
        }

        public override int GetHashCode()
        {
            return baseElement.GetHashCode();
        }

        public void remove()
        {
            throw new InvalidOperationException(ReadOnlyTokens.MUTATE_ERROR_MESSAGE);
        }

        public override bool Equals(object obj)
        {
            return ElementHelper.areEqual(this, obj);
        }
    }
}
