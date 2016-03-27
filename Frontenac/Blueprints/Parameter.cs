namespace Frontenac.Blueprints
{
    public class Parameter
    {
        public Parameter(object key, object value)
        {
            Key = key;
            Value = value;
        }

        public object Key { get; protected set; }

        public object Value { get; set; }

        public override bool Equals(object obj)
        {
            var parameter = obj as Parameter;
            if (parameter == null) return false;
            var param = parameter;
            var otherKey = param.Key;
            var otherValue = param.Value;
            if (otherKey == null)
            {
                if (Key != null)
                    return false;
            }
            else
            {
                if (!otherKey.Equals(Key))
                    return false;
            }

            if (otherValue == null)
            {
                if (Value != null)
                    return false;
            }
            else
            {
                if (!otherValue.Equals(Value))
                    return false;
            }

            return true;
        }

        public override int GetHashCode()
        {
            const int prime = 31;
            var result = 1;
// ReSharper disable NonReadonlyFieldInGetHashCode
            result = prime*result + (Key?.GetHashCode() ?? 0);
            result = prime*result + (Value?.GetHashCode() ?? 0);
// ReSharper restore NonReadonlyFieldInGetHashCode
            return result;
        }

        public override string ToString()
        {
            return $"parameter[{Key},{Value}]";
        }
    }

    public class Parameter<TK, TV> : Parameter
    {
        public Parameter(TK key, TV value)
            : base(key, value)
        {
        }

        public new TK Key => (TK) base.Key;

        public new TV Value
        {
            get { return (TV) base.Value; }
            set { base.Value = value; }
        }
    }

    public class NGram
    {
        public int Min { get; set; }
        public int Max { get; set; }
    }

    public class AutoCompleteParameter : Parameter<string, NGram>
    {
        public NGram NGram { get; set; }

        public AutoCompleteParameter(NGram value) : base("autocomplete", value)
        {
            NGram = value;
        }
    }
}