using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Frontenac.Blueprints.Util.Wrappers.Id
{
    public abstract class IdElement : Element
    {
        protected readonly Element _BaseElement;
        protected readonly IdGraph _IdGraph;
        protected readonly bool _PropertyBased;

        protected IdElement(Element baseElement, IdGraph idGraph, bool propertyBased)
        {
            _BaseElement = baseElement;
            _IdGraph = idGraph;
            _PropertyBased = propertyBased;
        }

        public object GetProperty(string key)
        {
            if (_PropertyBased && key == IdGraph.ID)
                return null;

            return _BaseElement.GetProperty(key);
        }

        public IEnumerable<string> GetPropertyKeys()
        {
            if (_PropertyBased)
            {
                IEnumerable<string> keys = _BaseElement.GetPropertyKeys();
                HashSet<string> s = new HashSet<string>(keys);
                s.Remove(IdGraph.ID);
                return s;
            }

            return _BaseElement.GetPropertyKeys();
        }

        public void SetProperty(string key, object value)
        {
            if (_PropertyBased && key == IdGraph.ID)
                throw new ArgumentException(string.Concat("Unable to set value for reserved property ", IdGraph.ID));

            _BaseElement.SetProperty(key, value);
        }

        public object RemoveProperty(string key)
        {
            if (_PropertyBased && key == IdGraph.ID)
                throw new ArgumentException(string.Concat("Unable to remove value for reserved property ", IdGraph.ID));

            return _BaseElement.RemoveProperty(key);
        }

        public object GetId()
        {
            return _PropertyBased
                ? _BaseElement.GetProperty(IdGraph.ID)
                : _BaseElement.GetId();
        }

        public override int GetHashCode()
        {
            return _BaseElement.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            return ElementHelper.AreEqual(this, obj);
        }

        public void Remove()
        {
            if (this is Vertex)
                _IdGraph.RemoveVertex((Vertex)this);
            else
                _IdGraph.RemoveEdge((Edge)this);
        }

        public override string ToString()
        {
            return _BaseElement.ToString();
        }
    }
}
