using System;
using System.Collections;
using Castle.Components.DictionaryAdapter;
using Frontenac.Gremlinq.RelationAccessors;

namespace Frontenac.Gremlinq
{
    public class DictionaryAdapterProxyFactory : IProxyFactory
    {
        private readonly PropertyDescriptor _propsDesc = new PropertyDescriptor();

        private readonly DictionaryAdapterFactory _dictionaryAdapterFactory = new DictionaryAdapterFactory();

        public DictionaryAdapterProxyFactory()
        {
            _propsDesc.AddBehavior(new ElementPropertyGetterBehavior());
        }

        public object Create(IDictionary element, Type proxyType)
        {
            return _dictionaryAdapterFactory.GetAdapter(proxyType, element, _propsDesc);
        }
    }
}