using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Frontenac.Blueprints
{
    public class Parameter
    {
        readonly object _Key;
        object _Value;

        public Parameter(object key, object value)
        {
            _Key = key;
            _Value = value;
        }

        public object GetKey()
        {
            return _Key;
        }

        public object GetValue()
        {
            return _Value;
        }

        public object SetValue(object value)
        {
            _Value = value;
            return value;
        }

        public override bool Equals(object obj)
        {
            if (obj is Parameter)
            {
                var param = obj as Parameter;
                object otherKey = param.GetKey();
                object otherValue = param.GetValue();
                if (otherKey == null)
                {
                    if (_Key != null)
                        return false;
                }
                else
                {
                    if (!otherKey.Equals(_Key))
                        return false;
                }

                if (otherValue == null)
                {
                    if (_Value != null)
                        return false;
                }
                else
                {
                    if (!otherValue.Equals(_Value))
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
            result = prime * result + ((_Key == null) ? 0 : _Key.GetHashCode());
            result = prime * result + ((_Value == null) ? 0 : _Value.GetHashCode());
            return result;
        }

        public override string ToString()
        {
            return string.Format("parameter[{0},{1}]", _Key, _Value);
        }
    }

    public class Parameter<K, V> : Parameter
    {
        public Parameter(K key, V value)
            : base(key, value)
        {

        }

        public new K GetKey()
        {
            return (K)base.GetKey();
        }

        public new V GetValue()
        {
            return (V)base.GetValue();
        }

        public V SetValue(V value)
        {
            return (V)base.SetValue(value);
        }
    }
}
