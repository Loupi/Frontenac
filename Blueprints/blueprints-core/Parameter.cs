using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Frontenac.Blueprints
{
    public class Parameter
    {
        readonly object _key;
        object _value;

        public Parameter(object key, object value)
        {
            _key = key;
            _value = value;
        }

        public object getKey()
        {
            return _key;
        }

        public object getValue()
        {
            return _value;
        }

        public object setValue(object value)
        {
            _value = value;
            return value;
        }

        public override bool Equals(object obj)
        {
            if (obj is Parameter)
            {
                var param = obj as Parameter;
                object otherKey = param.getKey();
                object otherValue = param.getValue();
                if (otherKey == null)
                {
                    if (_key != null)
                        return false;
                }
                else
                {
                    if (!otherKey.Equals(_key))
                        return false;
                }

                if (otherValue == null)
                {
                    if (_value != null)
                        return false;
                }
                else
                {
                    if (!otherValue.Equals(_value))
                        return false;
                }

                return true;
            }

            return false;
        }

        public override int GetHashCode()
        {
            const int prime = 31;
            int result = 1;
            result = prime * result + ((_key == null) ? 0 : _key.GetHashCode());
            result = prime * result + ((_value == null) ? 0 : _value.GetHashCode());
            return result;
        }

        public override string ToString()
        {
            return string.Format("parameter[{0},{1}]", _key, _value);
        }
    }

    public class Parameter<K, V> : Parameter
    {
        public Parameter(K key, V value)
            : base(key, value)
        {

        }

        public new K getKey()
        {
            return (K)base.getKey();
        }

        public new V getValue()
        {
            return (V)base.getValue();
        }

        public V setValue(V value)
        {
            return (V)base.setValue(value);
        }
    }
}
