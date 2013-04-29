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

        public object GetKey()
        {
            return _key;
        }

        public object GetValue()
        {
            return _value;
        }

        public object SetValue(object value)
        {
            _value = value;
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
// ReSharper disable NonReadonlyFieldInGetHashCode
            result = prime * result + ((_key == null) ? 0 : _key.GetHashCode());
            result = prime * result + ((_value == null) ? 0 : _value.GetHashCode());
// ReSharper restore NonReadonlyFieldInGetHashCode
            return result;
        }

        public override string ToString()
        {
            return string.Format("parameter[{0},{1}]", _key, _value);
        }
    }

    public class Parameter<TK, TV> : Parameter
    {
        public Parameter(TK key, TV value)
            : base(key, value)
        {

        }

        public new TK GetKey()
        {
            return (TK)base.GetKey();
        }

        public new TV GetValue()
        {
            return (TV)base.GetValue();
        }

        public TV GetValue(TV value)
        {
            return (TV)SetValue(value);
        }
    }
}
