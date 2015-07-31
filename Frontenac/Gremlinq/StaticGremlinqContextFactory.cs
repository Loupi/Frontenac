using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;

namespace Frontenac.Gremlinq
{
    public class StaticGremlinqContextFactory : IGremlinqContextFactory
    {
        private readonly DictionaryAdapterProxyFactory _proxyFactory = new DictionaryAdapterProxyFactory();
        private readonly DictionaryTypeProvider _typeProvider;

        public StaticGremlinqContextFactory(IDictionary<int, Type> types)
        {
            Contract.Requires(types != null);

            _typeProvider = new DictionaryTypeProvider(DictionaryTypeProvider.DefaulTypePropertyName, types);
        }

        public GremlinqContext Create()
        {
            return new GremlinqContext(_typeProvider, _proxyFactory);
        }
    }
}