namespace Frontenac.Gremlinq
{
    public class DefaultGremlinqContextFactory : IGremlinqContextFactory
    {
        readonly DictionaryAdapterProxyFactory _proxyFactory = new DictionaryAdapterProxyFactory();

        public virtual GremlinqContext Create()
        {
            var typeProvider = new GraphBackedTypeProvider(DictionaryTypeProvider.DefaulTypePropertyName);
            return new GremlinqContext(typeProvider, _proxyFactory);
        }
    }
}