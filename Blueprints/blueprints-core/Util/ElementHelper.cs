using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Frontenac.Blueprints.Util
{
    public static class ElementHelper
    {
        /// <summary>
        /// Determines whether the property key/value for the specified element can be legally set.
        /// This is typically used as a pre-condition check prior to setting a property.
        /// Throws ArgumentException whether the triple is legal and if not, a clear reason message is provided
        /// </summary>
        /// <param name="element">the element for the property to be set</param>
        /// <param name="key">the key of the property</param>
        /// <param name="value">the value of the property</param>
        public static void ValidateProperty(Element element, string key, object value)
        {
            if (null == value)
                throw ExceptionFactory.PropertyValueCanNotBeNull();
            if (null == key)
                throw ExceptionFactory.PropertyKeyCanNotBeNull();
            if (key.Equals(StringFactory.ID))
                throw ExceptionFactory.PropertyKeyIdIsReserved();
            if (element is Edge && key.Equals(StringFactory.LABEL))
                throw ExceptionFactory.PropertyKeyLabelIsReservedForEdges();
            if (string.IsNullOrEmpty(key))
                throw ExceptionFactory.PropertyKeyCanNotBeEmpty();
        }

        /// <summary>
        /// Copy the properties (key and value) from one element to another.
        /// The properties are preserved on the from element.
        /// ElementPropertiesRule that share the same key on the to element are overwritten.
        /// </summary>
        /// <param name="from">the element to copy properties from</param>
        /// <param name="to">the element to copy properties to</param>
        public static void CopyProperties(Element from, Element to)
        {
            foreach (string key in from.GetPropertyKeys())
                to.SetProperty(key, from.GetProperty(key));
        }

        /// <summary>
        /// Clear all the properties from an IEnumerable of elements.
        /// </summary>
        /// <param name="elements">the elements to remove properties from</param>
        public static void RemoveProperties(IEnumerable<Element> elements)
        {
            foreach (Element element in elements)
            {
                List<string> keys = new List<string>();
                keys.AddRange(element.GetPropertyKeys());
                foreach (string key in keys)
                    element.RemoveProperty(key);
            }
        }

        /// <summary>
        /// Remove a property from all elements in the provided IEnumerable.
        /// </summary>
        /// <param name="key">the property to remove by key</param>
        /// <param name="elements">the elements to remove the property from</param>
        public static void RemoveProperty(string key, IEnumerable<Element> elements)
        {
            foreach (Element element in elements)
                element.RemoveProperty(key);
        }

        /// <summary>
        /// Renames a property by removing the old key and adding the stored value to the new key.
        /// If property does not exist, nothing occurs.
        /// </summary>
        /// <param name="oldKey">the key to rename</param>
        /// <param name="newKey">the key to rename to</param>
        /// <param name="elements">the elements to rename</param>
        public static void RenameProperty(string oldKey, string newKey, IEnumerable<Element> elements)
        {
            foreach (Element element in elements)
            {
                object value = element.RemoveProperty(oldKey);
                if (null != value)
                    element.SetProperty(newKey, value);
            }
        }

        /// <summary>
        /// Typecasts a property value. This only works for casting to a class that has a constructor of the for new X(string).
        /// If no such constructor exists, an Exception is thrown and the original element property is left unchanged.
        /// </summary>
        /// <param name="key">the key for the property value to typecast</param>
        /// <param name="classCast">the class to typecast to</param>
        /// <param name="elements">the elements to have their property typecasted</param>
        public static void TypecastProperty(string key, Type classCast, IEnumerable<Element> elements)
        {
            foreach (Element element in elements)
            {
                object value = element.RemoveProperty(key);
                if (null != value)
                {
                    try
                    {
                        object convertedValue;
                        if (classCast.IsPrimitive)
                        {
                            TypeConverter tc = TypeDescriptor.GetConverter(classCast);
                            convertedValue = tc.ConvertFromString(value.ToString());
                        }
                        else
                            convertedValue = Activator.CreateInstance(classCast, value.ToString());

                        element.SetProperty(key, convertedValue);
                    }
                    catch (Exception)
                    {
                        element.SetProperty(key, value);
                        throw;
                    }
                }
            }
        }

        /// <summary>
        /// Determines whether two elements have the same properties.
        /// To be true, both must have the same property keys and respective values must be equals().
        /// </summary>
        /// <param name="a">an element</param>
        /// <param name="b">an element</param>
        /// <returns>whether the two elements have equal properties</returns>
        public static bool HaveEqualProperties(Element a, Element b)
        {
            IEnumerable<string> aKeys = a.GetPropertyKeys();
            IEnumerable<string> bKeys = b.GetPropertyKeys();

            if (aKeys.Intersect(bKeys).LongCount() == bKeys.LongCount() && bKeys.Intersect(aKeys).LongCount() == aKeys.LongCount())
            {
                foreach (string key in aKeys)
                {
                    if (!a.GetProperty(key).Equals(b.GetProperty(key)))
                        return false;
                }
                return true;
            }

            return false;
        }

        /// <summary>
        /// Get a clone of the properties of the provided element.
        /// In other words, a HashMap is created and filled with the key/values of the element's properties.
        /// </summary>
        /// <param name="element">the element to get the properties of</param>
        /// <returns>a clone of the properties of the element</returns>
        public static IDictionary<string, object> GetProperties(Element element)
        {
            Dictionary<string, object> properties = new Dictionary<string, object>();
            foreach (string key in element.GetPropertyKeys())
                properties.Add(key, element.GetProperty(key));
            return properties;
        }

        /// <summary>
        /// Set the properties of the provided element using the provided dictionary.
        /// </summary>
        /// <param name="element">the element to set the properties of</param>
        /// <param name="properties">the properties to set as a IDictionary<string, object></param>
        public static void SetProperties(Element element, IDictionary<string, object> properties)
        {
            foreach (var property in properties)
                element.SetProperty(property.Key, property.Value);
        }

        /// <summary>
        /// Set the properties of the provided element using the provided key value pairs.
        /// The var args of Objects must be divisible by 2. All odd elements in the array must be a string key.
        /// </summary>
        /// <param name="element">the element to set the properties of</param>
        /// <param name="keysValues">the key value pairs of the properties</param>
        public static void SetProperties(Element element, params object[] keysValues)
        {
            if (keysValues.Length % 2 != 0)
                throw new ArgumentException("The object var args must be divisible by 2");
            for (int i = 0; i < keysValues.Length; i = i + 2)
                element.SetProperty((string)keysValues[i], keysValues[i + 1]);
        }

        /// <summary>
        /// A standard method for determining if two elements are equal.
        /// This method should be used by any Element.equals() implementation to ensure consistent behavior.
        /// </summary>
        /// <param name="a">The first element</param>
        /// <param name="b">The second element (as an object)</param>
        /// <returns>Whether the two elements are equal</returns>
        public static bool AreEqual(Element a, object b)
        {
            if (a == b)
                return true;
            if (null == b)
                return false;
            if (!a.GetType().Equals(b.GetType()))
                return false;
            return a.GetId().Equals(((Element)b).GetId());
        }

        /// <summary>
        /// Simply tests if the element ids are equal().
        /// </summary>
        /// <param name="a">the first element</param>
        /// <param name="b">the second element</param>
        /// <returns>Whether the two elements have equal ids</returns>
        public static bool HaveEqualIds(Element a, Element b)
        {
            return a.GetId().Equals(b.GetId());
        }
    }
}
