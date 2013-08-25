using System.Collections.Generic;

namespace Frontenac.Blueprints.Util.Wrappers.Wrapped
{
    public abstract class WrappedElement : IElement
    {
        protected readonly IElement BaseElement;

        protected WrappedElement(IElement baseElement)
        {
            BaseElement = baseElement;
        }

        public void SetProperty(string key, object value)
        {
            BaseElement.SetProperty(key, value);
        }

        public object GetProperty(string key)
        {
            return BaseElement.GetProperty(key);
        }

        public object RemoveProperty(string key)
        {
            return BaseElement.RemoveProperty(key);
        }

        public IEnumerable<string> GetPropertyKeys()
        {
            return BaseElement.GetPropertyKeys();
        }

        public object Id
        {
            get { return BaseElement.Id; }
        }

        public override bool Equals(object obj)
        {
            return ElementHelper.AreEqual(this, obj);
        }

        public override int GetHashCode()
        {
            return BaseElement.GetHashCode();
        }

        public IElement GetBaseElement()
        {
            return BaseElement;
        }

        public void Remove()
        {
            BaseElement.Remove();
        }

        public override string ToString()
        {
            return BaseElement.ToString();
        }
    }
}
