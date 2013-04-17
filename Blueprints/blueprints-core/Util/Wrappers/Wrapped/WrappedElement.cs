using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Frontenac.Blueprints.Util.Wrappers.Wrapped
{
    public abstract class WrappedElement : Element
    {
        protected Element _BaseElement;

        protected WrappedElement(Element baseElement)
        {
            _BaseElement = baseElement;
        }

        public void SetProperty(string key, object value)
        {
            _BaseElement.SetProperty(key, value);
        }

        public object GetProperty(string key)
        {
            return _BaseElement.GetProperty(key);
        }

        public object RemoveProperty(string key)
        {
            return _BaseElement.RemoveProperty(key);
        }

        public IEnumerable<string> GetPropertyKeys()
        {
            return _BaseElement.GetPropertyKeys();
        }

        public object GetId()
        {
            return _BaseElement.GetId();
        }

        public override bool Equals(object obj)
        {
            return ElementHelper.AreEqual(this, obj);
        }

        public override int GetHashCode()
        {
            return _BaseElement.GetHashCode();
        }

        public Element GetBaseElement()
        {
            return _BaseElement;
        }

        public void Remove()
        {
            _BaseElement.Remove();
        }

        public override string ToString()
        {
            return _BaseElement.ToString();
        }
    }
}
