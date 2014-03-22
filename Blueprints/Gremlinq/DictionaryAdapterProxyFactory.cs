using System;
using Castle.Components.DictionaryAdapter;
using Frontenac.Blueprints;

namespace Frontenac.Gremlinq
{
    public class DictionaryAdapterProxyFactory : IProxyFactory
    {
        readonly PropertyDescriptor _propsDesc = new PropertyDescriptor();
        readonly DictionaryAdapterFactory _dictionaryAdapterFactory = new DictionaryAdapterFactory();

        public DictionaryAdapterProxyFactory()
        {
            _propsDesc.AddBehavior(new DictionaryPropertyConverter());
        }

        public object Create(IElement element, Type proxyType)
        {
            return _dictionaryAdapterFactory.GetAdapter(proxyType, element, _propsDesc);
        }
    }
}