using System;

namespace Frontenac.Blueprints.Util.Wrappers.Id
{
    public class IdEdgeIndex : IIndex
    {
        protected readonly IIndex BaseIndex;
        protected readonly IdGraph IdGraph;

        public IdEdgeIndex(IIndex baseIndex, IdGraph idGraph)
        {
            if (null == baseIndex)
                throw new ArgumentException("null base index");

            IdGraph = idGraph;
            BaseIndex = baseIndex;
        }

        public string GetIndexName()
        {
            return BaseIndex.GetIndexName();
        }

        public Type GetIndexClass()
        {
            return BaseIndex.GetIndexClass();
        }

        public void Put(string key, object value, IElement element)
        {
            BaseIndex.Put(key, value, GetBaseElement(element));
        }

        public ICloseableIterable<IElement> Get(string key, object value)
        {
            return new IdEdgeIterable(BaseIndex.Get(key, value), IdGraph);
        }

        public ICloseableIterable<IElement> Query(string key, object value)
        {
            return new IdEdgeIterable(BaseIndex.Query(key, value), IdGraph);
        }

        public long Count(string key, object value)
        {
            return BaseIndex.Count(key, value);
        }

        public void Remove(string key, object value, IElement element)
        {
            BaseIndex.Remove(key, value, GetBaseElement(element));
        }

        public override string ToString()
        {
            return StringFactory.IndexString(this);
        }

        IEdge GetBaseElement(IElement e)
        {
            return ((IdEdge)e).GetBaseEdge();
        }
    }
}
