using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Frontenac.Blueprints.Util
{
    /// <summary>
    /// The ExceptionFactory provides standard exceptions that should be used by all Blueprints implementations.
    /// This ensures that the look-and-feel of all implementations are the same in terms of terminology and punctuation.
    /// </summary>
    public static class ExceptionFactory
    {
        // Graph related exceptions

        public static ArgumentException vertexIdCanNotBeNull()
        {
            return new ArgumentException("Vertex id can not be null");
        }

        public static ArgumentException edgeIdCanNotBeNull()
        {
            return new ArgumentException("Edge id can not be null");
        }

        public static ArgumentException vertexWithIdAlreadyExists(object id)
        {
            return new ArgumentException(string.Format("Vertex with id already exists: {0}", id));
        }

        public static ArgumentException edgeWithIdAlreadyExist(object id)
        {
            return new ArgumentException(string.Format("Edge with id already exists: {0}", id));
        }

        public static ArgumentException bothIsNotSupported()
        {
            return new ArgumentException("A direction of BOTH is not supported");
        }

        // Element related exceptions

        public static ArgumentException propertyKeyIsReserved(string key)
        {
            return new ArgumentException(string.Format("Property key is reserved for all elements: {0}", key));
        }

        public static ArgumentException propertyKeyIdIsReserved()
        {
            return new ArgumentException("Property key is reserved for all elements: id");
        }

        public static ArgumentException propertyKeyLabelIsReservedForEdges()
        {
            return new ArgumentException("Property key is reserved for all edges: label");
        }

        public static ArgumentException propertyKeyCanNotBeEmpty()
        {
            return new ArgumentException("Property key can not be the empty string");
        }

        public static ArgumentException propertyKeyCanNotBeNull()
        {
            return new ArgumentException("Property key can not be null");
        }

        public static ArgumentException propertyValueCanNotBeNull()
        {
            return new ArgumentException("Property value can not be null");
        }

        // IndexableGraph related exceptions

        public static ArgumentException indexAlreadyExists(string indexName)
        {
            return new ArgumentException(string.Format("Index already exists: {0}", indexName));
        }

        public static InvalidOperationException indexDoesNotSupportClass(string indexName, Type clazz)
        {
            return new InvalidOperationException(string.Format("{0} does not support class: {1}", indexName, clazz));
        }

        // KeyIndexableGraph related exceptions

        public static ArgumentException classIsNotIndexable(Type clazz)
        {
            return new ArgumentException(string.Format("Class is not indexable: {0}", clazz));
        }

        // TransactionalGraph related exceptions

        public static InvalidOperationException transactionAlreadyStarted()
        {
            return new InvalidOperationException("Stop the current transaction before starting another");
        }
    }
}
