using System;

namespace Frontenac.Blueprints.Util
{
    /// <summary>
    ///     The ExceptionFactory provides standard exceptions that should be used by all Blueprints implementations.
    ///     This ensures that the look-and-feel of all implementations are the same in terms of terminology and punctuation.
    /// </summary>
    public static class ExceptionFactory
    {
        // Graph related exceptions

        public static ArgumentException VertexIdCanNotBeNull()
        {
            return new ArgumentException("Vertex id can not be null");
        }

        public static ArgumentException EdgeIdCanNotBeNull()
        {
            return new ArgumentException("Edge id can not be null");
        }

        public static ArgumentException VertexWithIdAlreadyExists(object id)
        {
            return new ArgumentException($"Vertex with id already exists: {id}");
        }

        public static ArgumentException EdgeWithIdAlreadyExist(object id)
        {
            return new ArgumentException($"Edge with id already exists: {id}");
        }

        public static ArgumentException BothIsNotSupported()
        {
            return new ArgumentException("A direction of BOTH is not supported");
        }

        // Element related exceptions

        public static ArgumentException PropertyKeyIsReserved(string key)
        {
            return new ArgumentException($"Property key is reserved for all elements: {key}");
        }

        public static ArgumentException PropertyKeyIdIsReserved()
        {
            return new ArgumentException("Property key is reserved for all elements: id");
        }

        public static ArgumentException PropertyKeyLabelIsReservedForEdges()
        {
            return new ArgumentException("Property key is reserved for all edges: label");
        }

        public static ArgumentException PropertyKeyCanNotBeEmpty()
        {
            return new ArgumentException("Property key can not be the empty string");
        }

        public static ArgumentException PropertyKeyCanNotBeNull()
        {
            return new ArgumentException("Property key can not be null");
        }

        public static ArgumentException PropertyValueCanNotBeNull()
        {
            return new ArgumentException("Property value can not be null");
        }

        // IIndexableGraph related exceptions

        public static ArgumentException IndexAlreadyExists(string indexName)
        {
            return new ArgumentException($"Index already exists: {indexName}");
        }

        public static InvalidOperationException IndexDoesNotSupportClass(string indexName, Type clazz)
        {
            return new InvalidOperationException($"{indexName} does not support class: {clazz}");
        }

        // KeyIndexableGraph related exceptions

        public static ArgumentException ClassIsNotIndexable(Type clazz)
        {
            return new ArgumentException($"Class is not indexable: {clazz}");
        }

        // TransactionalGraph related exceptions

        public static InvalidOperationException TransactionAlreadyStarted()
        {
            return new InvalidOperationException("Stop the current transaction before starting another");
        }
    }
}