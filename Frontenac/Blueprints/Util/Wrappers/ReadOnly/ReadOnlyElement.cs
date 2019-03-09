using System;
using System.Collections.Generic;
using Frontenac.Blueprints.Contracts;

namespace Frontenac.Blueprints.Util.Wrappers.ReadOnly
{
    public abstract class ReadOnlyElement : DictionaryElement
    {
        protected readonly IElement BaseElement;
        protected readonly ReadOnlyGraph ReadOnlyGraph;

        protected ReadOnlyElement(ReadOnlyGraph graph, IElement baseElement):base(graph)
        {
            if (graph == null)
                throw new ArgumentNullException(nameof(graph));
            if (baseElement == null)
                throw new ArgumentNullException(nameof(baseElement));

            IsReadOnly = true;
            BaseElement = baseElement;
            ReadOnlyGraph = graph;
        }

        public override IEnumerable<string> GetPropertyKeys()
        {
            return BaseElement.GetPropertyKeys();
        }

        public override object Id => BaseElement.Id;

        public override object RemoveProperty(string key)
        {
            ElementContract.ValidateRemoveProperty(key);
            throw new InvalidOperationException(ReadOnlyTokens.MutateErrorMessage);
        }

        public override object GetProperty(string key)
        {
            ElementContract.ValidateGetProperty(key);
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