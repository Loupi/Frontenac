using System;
using System.ComponentModel;
using Castle.Components.DictionaryAdapter;
using PropertyDescriptor = Castle.Components.DictionaryAdapter.PropertyDescriptor;

namespace Grave
{
    class DictionaryPropertyConverter : DictionaryBehaviorAttribute, IDictionaryPropertyGetter
    {
        public object GetPropertyValue(IDictionaryAdapter dictionaryAdapter, string key, object storedValue,
                                       PropertyDescriptor property, bool ifExists)
        {
            if (null == storedValue || property.PropertyType.IsInstanceOfType(storedValue))
                return storedValue;

            object convertedValue;
            if (property.PropertyType.IsPrimitive)
            {
                var tc = TypeDescriptor.GetConverter(property.PropertyType);
                convertedValue = tc.ConvertFromString(storedValue.ToString());
            }
            else
                convertedValue = Activator.CreateInstance(property.PropertyType, storedValue);

            return convertedValue;
        }
    }
}