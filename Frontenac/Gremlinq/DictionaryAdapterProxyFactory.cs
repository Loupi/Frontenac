using System;
using System.Collections;
using Castle.Components.DictionaryAdapter;
using Frontenac.Gremlinq.Contracts;
using Frontenac.Gremlinq.RelationAccessors;

namespace Frontenac.Gremlinq
{
    public class DictionaryAdapterProxyFactory : IProxyFactory
    {
        readonly PropertyDescriptor _propsDesc = new PropertyDescriptor();

        readonly DictionaryAdapterFactory _dictionaryAdapterFactory = new DictionaryAdapterFactory();

        public DictionaryAdapterProxyFactory()
        {
            _propsDesc.AddBehavior(new ElementPropertyGetterBehavior());
        }

        public object Create(IDictionary element, Type proxyType)
        {
            ProxyFactoryContract.ValidateCreate(element, proxyType);

            return _dictionaryAdapterFactory.GetAdapter(proxyType, element, _propsDesc);
        }
    }
}