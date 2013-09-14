using System;
using System.Diagnostics.Contracts;

namespace Frontenac.Blueprints.Util.Wrappers.Id
{
    public class IdVertexIndex : IIndex
    {
        private readonly IIndex _baseIndex;
        private readonly IdGraph _idGraph;

        public IdVertexIndex(IIndex baseIndex, IdGraph idGraph)
        {
            Contract.Requires(baseIndex != null);
            Contract.Requires(idGraph != null);

            _idGraph = idGraph;
            _baseIndex = baseIndex;
        }

        public string Name
        {
            get { return _baseIndex.Name; }
        }

        public Type Type
        {
            get { return _baseIndex.Type; }
        }

        public void Put(string key, object value, IElement element)
        {
            _baseIndex.Put(key, value, GetBaseElement(element));
        }

        public ICloseableIterable<IElement> Get(string key, object value)
        {
            return new IdVertexIterable(_baseIndex.Get(key, value), _idGraph);
        }

        public ICloseableIterable<IElement> Query(string key, object value)
        {
            return new IdVertexIterable(_baseIndex.Query(key, value), _idGraph);
        }

        public long Count(string key, object value)
        {
            return _baseIndex.Count(key, value);
        }

        public void Remove(string key, object value, IElement element)
        {
            _baseIndex.Remove(key, value, GetBaseElement(element));
        }

        private IVertex GetBaseElement(IElement e)
        {
            Contract.Requires(e is IdVertex);
            Contract.Ensures(Contract.Result<IVertex>() != null);
            return ((IdVertex) e).GetBaseVertex();
        }

        public override string ToString()
        {
            return StringFactory.IndexString(this);
        }
    }
}