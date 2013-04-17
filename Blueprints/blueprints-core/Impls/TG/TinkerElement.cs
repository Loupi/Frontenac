using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Frontenac.Blueprints.Util;

namespace Frontenac.Blueprints.Impls.TG
{
    [Serializable]
    abstract class TinkerElement : Element
    {
        protected Dictionary<string, object> _Properties = new Dictionary<string, object>();
        protected readonly string _Id;
        protected readonly TinkerGraph _Graph;

        protected TinkerElement(string id, TinkerGraph graph)
        {
            _Graph = graph;
            _Id = id;
        }

        public IEnumerable<string> GetPropertyKeys()
        {
            return new HashSet<string>(_Properties.Keys);
        }

        public object GetProperty(string key)
        {
            return _Properties.Get(key);
        }

        public void SetProperty(string key, object value)
        {
            ElementHelper.ValidateProperty(this, key, value);
            object oldValue = _Properties.Put(key, value);
            if (this is TinkerVertex)
                _Graph._VertexKeyIndex.AutoUpdate(key, value, oldValue, (TinkerVertex)this);
            else
                _Graph._EdgeKeyIndex.AutoUpdate(key, value, oldValue, (TinkerEdge)this);
        }

        public object RemoveProperty(string key)
        {
            object oldValue = _Properties.JavaRemove(key);
            if (this is TinkerVertex)
                _Graph._VertexKeyIndex.AutoRemove(key, oldValue, (TinkerVertex)this);
            else
                _Graph._EdgeKeyIndex.AutoRemove(key, oldValue, (TinkerEdge)this);

            return oldValue;
        }

        public override int GetHashCode()
        {
            return _Id.GetHashCode();
        }

        public object GetId()
        {
            return _Id;
        }

        public override bool Equals(object obj)
        {
            return ElementHelper.AreEqual(this, obj);
        }

        public void Remove()
        {
            if (this is Vertex)
                _Graph.RemoveVertex((Vertex)this);
            else
                _Graph.RemoveEdge((Edge)this);
        }
    }
}
