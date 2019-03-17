using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace MmGraph.Database.Records
{
    public enum PropertyType
    {
        Null,
        Boolean,
        Byte,
        Sbyte,
        Char,
        Decimal,
        Double,
        Float,
        Int,
        Uint,
        Long,
        Ulong,
        Short,
        Ushort,
        String,
        BooleanArray,
        ByteArray,
        SbyteArray,
        CharArray,
        DoubleArray,
        FloatArray,
        IntArray,
        UintArray,
        LongArray,
        UlongArray,
        ShortArray,
        UshortArray,
        StringArray
    }

    public static class PropertyTypeHelpers
    {
        private static readonly Dictionary<Type, PropertyType> PropertyTypes = new Dictionary<Type, PropertyType>()
        {
            { typeof(bool), PropertyType.Boolean },
            { typeof(byte), PropertyType.Byte },
            { typeof(sbyte), PropertyType.Sbyte },
            { typeof(char), PropertyType.Char },
            { typeof(decimal), PropertyType.Decimal },
            { typeof(double), PropertyType.Double },
            { typeof(float), PropertyType.Float },
            { typeof(int), PropertyType.Int },
            { typeof(uint), PropertyType.Uint },
            { typeof(long), PropertyType.Long },
            { typeof(ulong), PropertyType.Ulong },
            { typeof(short), PropertyType.Short },
            { typeof(ushort), PropertyType.Ushort },
            { typeof(string), PropertyType.String }
        };

        private static readonly Dictionary<Type, PropertyType> ArrayPropertyTypes = new Dictionary<Type, PropertyType>()
        {
            { typeof(bool), PropertyType.BooleanArray },
            { typeof(byte), PropertyType.ByteArray },
            { typeof(sbyte), PropertyType.SbyteArray },
            { typeof(char), PropertyType.CharArray },
            { typeof(decimal), PropertyType.Decimal },
            { typeof(double), PropertyType.DoubleArray },
            { typeof(float), PropertyType.FloatArray },
            { typeof(int), PropertyType.Int },
            { typeof(uint), PropertyType.Uint },
            { typeof(long), PropertyType.Long },
            { typeof(ulong), PropertyType.Ulong },
            { typeof(short), PropertyType.Short },
            { typeof(ushort), PropertyType.Ushort },
            { typeof(string), PropertyType.String }
        };

        private static Type GetElementTypeOfEnumerable(object o)
        {
            // if it's not an enumerable why do you call this method all ?
            if (!(o is IEnumerable enumerable))
                return null;

            var interfaces = enumerable.GetType().GetInterfaces();
            var elementType = interfaces
                .Where(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IEnumerable<>))
                .Select(i => i.GetGenericArguments()[0])
                .FirstOrDefault();

            //peek at the first element in the list if you couldn't determine the element type
            if (elementType == null || elementType == typeof(object))
            {
                var firstElement = enumerable.Cast<object>().FirstOrDefault();
                if (firstElement != null)
                    elementType = firstElement.GetType();
            }
            return elementType;
        }

        public static PropertyType GetPropertyType(object value)
        {
            if (value == null)
                return PropertyType.Null;

            var type = value.GetType();
            if (PropertyTypes.TryGetValue(type, out var result))
                return result;

            type = GetElementTypeOfEnumerable(value);
            if (type != null && ArrayPropertyTypes.TryGetValue(type, out result))
                return result;

            throw new NotSupportedException(value.GetType().Name);
        }
    }
}
