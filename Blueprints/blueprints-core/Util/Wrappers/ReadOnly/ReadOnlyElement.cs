﻿using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;

namespace Frontenac.Blueprints.Util.Wrappers.ReadOnly
{
    public abstract class ReadOnlyElement : IElement
    {
        protected readonly IElement BaseElement;

        protected ReadOnlyElement(IElement baseElement)
        {
            Contract.Requires(baseElement != null);

            BaseElement = baseElement;
        }

        public IEnumerable<string> GetPropertyKeys()
        {
            return BaseElement.GetPropertyKeys();
        }

        public object Id
        {
            get { return BaseElement.Id; }
        }

        public object RemoveProperty(string key)
        {
            throw new InvalidOperationException(ReadOnlyTokens.MutateErrorMessage);
        }

        public object GetProperty(string key)
        {
            return BaseElement.GetProperty(key);
        }

        public void SetProperty(string key, object value)
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

        public void Remove()
        {
            throw new InvalidOperationException(ReadOnlyTokens.MutateErrorMessage);
        }

        public override bool Equals(object obj)
        {
            return ElementHelper.AreEqual(this, obj);
        }
    }
}
