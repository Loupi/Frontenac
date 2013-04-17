using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Frontenac.Blueprints.Util.Wrappers.ReadOnly
{
    public abstract class ReadOnlyElement : Element
    {
        protected Element _BaseElement;

        protected ReadOnlyElement(Element baseElement)
        {
            _BaseElement = baseElement;
        }

        public IEnumerable<string> GetPropertyKeys()
        {
            return _BaseElement.GetPropertyKeys();
        }

        public object GetId()
        {
            return _BaseElement.GetId();
        }

        public object RemoveProperty(string key)
        {
            throw new InvalidOperationException(ReadOnlyTokens.MUTATE_ERROR_MESSAGE);
        }

        public object GetProperty(string key)
        {
            return _BaseElement.GetProperty(key);
        }

        public void SetProperty(string key, object value)
        {
            throw new InvalidOperationException(ReadOnlyTokens.MUTATE_ERROR_MESSAGE);
        }

        public override string ToString()
        {
            return _BaseElement.ToString();
        }

        public override int GetHashCode()
        {
            return _BaseElement.GetHashCode();
        }

        public void Remove()
        {
            throw new InvalidOperationException(ReadOnlyTokens.MUTATE_ERROR_MESSAGE);
        }

        public override bool Equals(object obj)
        {
            return ElementHelper.AreEqual(this, obj);
        }
    }
}
