using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;

namespace Frontenac.Blueprints.Util.Wrappers.ReadOnly
{
    public abstract class ReadOnlyElement : DictionaryElement
    {
        protected readonly IElement BaseElement;

        protected ReadOnlyElement(IElement baseElement)
        {
            Contract.Requires(baseElement != null);
            IsReadOnly = true;

            BaseElement = baseElement;
        }

        public override IEnumerable<string> GetPropertyKeys()
        {
            return BaseElement.GetPropertyKeys();
        }

        public override object Id
        {
            get { return BaseElement.Id; }
        }

        public override object RemoveProperty(string key)
        {
            throw new InvalidOperationException(ReadOnlyTokens.MutateErrorMessage);
        }

        public override object GetProperty(string key)
        {
            return BaseElement.GetProperty(key);
        }

        public override void SetProperty(string key, object value)
        {
            throw new InvalidOperationException(ReadOnlyTokens.MutateErrorMessage);
        }

        public override void Remove()
        {
            throw new InvalidOperationException(ReadOnlyTokens.MutateErrorMessage);
        }

        public override string ToString()
        {
            return BaseElement.ToString();
        }

        public override int GetHashCode()
        {
            return BaseElement.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            return this.AreEqual(obj);
        }
    }
}