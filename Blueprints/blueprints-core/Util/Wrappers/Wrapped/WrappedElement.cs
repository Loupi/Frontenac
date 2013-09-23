using System.Collections.Generic;
using System.Diagnostics.Contracts;

namespace Frontenac.Blueprints.Util.Wrappers.Wrapped
{
    public abstract class WrappedElement : DictionaryElement
    {
        protected readonly IElement BaseElement;

        protected WrappedElement(IElement baseElement)
        {
            Contract.Requires(baseElement != null);

            BaseElement = baseElement;
        }

        public override void SetProperty(string key, object value)
        {
            BaseElement.SetProperty(key, value);
        }

        public override object GetProperty(string key)
        {
            return BaseElement.GetProperty(key);
        }

        public override object RemoveProperty(string key)
        {
            return BaseElement.RemoveProperty(key);
        }

        public override IEnumerable<string> GetPropertyKeys()
        {
            return BaseElement.GetPropertyKeys();
        }

        public override object Id
        {
            get { return BaseElement.Id; }
        }

        public override void Remove()
        {
            BaseElement.Remove();
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
            Contract.Ensures(Contract.Result<IElement>() != null);
            return BaseElement;
        }

        public override string ToString()
        {
            return BaseElement.ToString();
        }
    }
}