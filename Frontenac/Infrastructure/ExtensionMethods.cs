using System;

namespace Frontenac.Infrastructure
{
    public static class ExtensionMethods
    {
        public static int ToInt32(this object value)
        {
            // ReSharper disable PossibleInvalidOperationException
            var result = value.TryToInt32();
            if (result.HasValue)
                return result.Value;
            return -1;
            // ReSharper restore PossibleInvalidOperationException
        }

        public static int? TryToInt32(this object value)
        {
            int? result;

            if (value is int)
                result = (int)value;
            else
            {
                var s = value as string;
                if (s != null)
                {
                    int intVal;
                    result = int.TryParse(s, out intVal) ? (int?)intVal : null;
                }
                else if (value == null)
                    result = null;
                else
                {
                    try
                    {
                        result = Convert.ToInt32(value);
                    }
                    catch (Exception ex)
                    {
                        if (ex is FormatException || ex is InvalidCastException || ex is OverflowException)
                            result = null;
                        else
                            throw;
                    }
                }
            }

            return result;
        }

        public static long ToInt64(this object value)
        {
// ReSharper disable PossibleInvalidOperationException
            return value.TryToInt64().Value;
// ReSharper restore PossibleInvalidOperationException
        }

        public static long? TryToInt64(this object value)
        {
            long? result;

            if (value is long)
                result = (long)value;
            else
            {
                var s = value as string;
                if (s != null)
                {
                    long intVal;
                    result = long.TryParse(s, out intVal) ? (long?)intVal : null;
                }
                else if (value == null)
                    result = null;
                else
                {
                    try
                    {
                        result = Convert.ToInt64(value);
                    }
                    catch (Exception ex)
                    {
                        if (ex is FormatException || ex is InvalidCastException || ex is OverflowException)
                            result = null;
                        else
                            throw;
                    }
                }
            }

            return result;
        }
    }
}
