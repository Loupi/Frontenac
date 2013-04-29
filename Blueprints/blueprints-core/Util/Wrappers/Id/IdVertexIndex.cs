using System;

namespace Frontenac.Blueprints.Util.Wrappers.Id
{
    public class IdVertexIndex : IIndex
    {
        readonly IIndex _baseIndex;
        readonly IdGraph _idGraph;

        public IdVertexIndex(IIndex baseIndex, IdGraph idGraph)
        {
            if (null == baseIndex)
                throw new ArgumentException("null base index");

            _idGraph = idGraph;
            _baseIndex = baseIndex;
        }

        public string GetIndexName()
        {
            return _baseIndex.GetIndexName();
        }

        public Type GetIndexClass()
        {
            return _baseIndex.GetIndexClass();
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

        IVertex GetBaseElement(IElement e)
        {
            return ((IdVertex)e).GetBaseVertex();
        }

        public override string ToString()
        {
            return StringFactory.IndexString(this);
        }
    }
}
