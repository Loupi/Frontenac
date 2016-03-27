using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;

namespace Frontenac.Blueprints.Util.Wrappers.ReadOnly
{
    public abstract class ReadOnlyElement : DictionaryElement
    {
        protected readonly IElement BaseElement;
        protected readonly ReadOnlyGraph ReadOnlyInnerTinkerGrapĥ;

        protected ReadOnlyElement(ReadOnlyGraph innerTinkerGrapĥ, IElement baseElement):base(innerTinkerGrapĥ)
        {
            Contract.Requires(innerTinkerGrapĥ != null);
            Contract.Requires(baseElement != null);

            IsReadOnly = true;
            BaseElement = baseElement;
            ReadOnlyInnerTinkerGrapĥ = innerTinkerGrapĥ;
        }

        public override IEnumerable<string> GetPropertyKeys()
        {
            return BaseElement.GetPropertyKeys();
        }

        public override object Id => BaseElement.Id;

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