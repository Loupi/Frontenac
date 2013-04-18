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
        protected Dictionary<string, object> properties = new Dictionary<string, object>();
        protected readonly string id;
        protected readonly TinkerGraph graph;

        protected TinkerElement(string id, TinkerGraph graph)
        {
            this.graph = graph;
            this.id = id;
        }

        public IEnumerable<string> getPropertyKeys()
        {
            return new HashSet<string>(properties.Keys);
        }

        public object getProperty(string key)
        {
            return properties.get(key);
        }

        public void setProperty(string key, object value)
        {
            ElementHelper.validateProperty(this, key, value);
            object oldValue = properties.put(key, value);
            if (this is TinkerVertex)
                graph.vertexKeyIndex.autoUpdate(key, value, oldValue, (TinkerVertex)this);
            else
                graph.edgeKeyIndex.autoUpdate(key, value, oldValue, (TinkerEdge)this);
        }

        public object removeProperty(string key)
        {
            object oldValue = properties.javaRemove(key);
            if (this is TinkerVertex)
                graph.vertexKeyIndex.autoRemove(key, oldValue, (TinkerVertex)this);
            else
                graph.edgeKeyIndex.autoRemove(key, oldValue, (TinkerEdge)this);

            return oldValue;
        }

        public override int GetHashCode()
        {
            return id.GetHashCode();
        }

        public object getId()
        {
            return id;
        }

        public override bool Equals(object obj)
        {
            return ElementHelper.areEqual(this, obj);
        }

        public void remove()
        {
            if (this is Vertex)
                graph.removeVertex((Vertex)this);
            else
                graph.removeEdge((Edge)this);
        }
    }
}
