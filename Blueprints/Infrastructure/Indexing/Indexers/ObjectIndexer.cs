using System;
using System.Collections;
using System.Diagnostics.Contracts;
using System.Security.Principal;
using System.Text;
using Fasterflect;
using Frontenac.Infrastructure.Properties;

namespace Frontenac.Infrastructure.Indexing.Indexers
{
    public class ObjectIndexer : Indexer
    {
        private readonly IIndexerFactory _indexerFactory;
        private static readonly uint MaxDepth = Settings.Default.ObjectIndexerMaxDepth;

        public ObjectIndexer(IIndexerFactory indexerFactory)
        {
            Contract.Requires(indexerFactory != null);
            
            _indexerFactory = indexerFactory;
        }

        public override void Index(IDocument document, string name, object content)
        {
            var nameBuilder = new StringBuilder(name);
            Recurse(0, document, content, nameBuilder);
        }

        private void Recurse(uint depth, IDocument document, object value, StringBuilder nameBuilder)
        {
            Contract.Requires(value != null);

            var type = value.GetType();

            if (!type.IsValueType && document.Present(value))
                return;

            var name = nameBuilder.ToString();

            if (document.Write(name, value))
                return;

            var indexer = _indexerFactory.Create(type);
            try
            {
                if (!(indexer is ObjectIndexer))
                {
                    indexer.Index(document, name, value);
                    return;
                }
            }
            finally 
            {
                _indexerFactory.Destroy(indexer);
            }
            
            if ((type.IsValueType && type.FullName == string.Concat("System.", type.Name)) ||
                (type.FullName == string.Concat("System.Reflection.", type.Name)) ||
                (value is SecurityIdentifier))
                return;

            if (value is IEnumerable)
            {
                foreach (var obj in value as IEnumerable)
                {
                    Recurse(depth + 1, document, obj, nameBuilder);
                }
                return;
            }

            var properties = type.Properties(Flags.InstancePublic);
            var fields = type.Fields(Flags.InstancePublic);

            if (properties.Count == 0 && fields.Count == 0)
            {
                var stringValue = value.ToString();
                if (stringValue != type.FullName)
                    document.Write(name, stringValue);
            }

            if (depth >= MaxDepth) return;

            foreach (var property in properties)
            {
                if (nameBuilder.Length > 0)
                    nameBuilder.Append(".");
                nameBuilder.Append(property.Name);
                Recurse(depth + 1, document, value.GetPropertyValue(property.Name), nameBuilder);
                nameBuilder.Remove(name.Length, nameBuilder.Length - name.Length);
            }

            foreach (var field in fields)
            {
                if (nameBuilder.Length > 0)
                    nameBuilder.Append(".");
                nameBuilder.Append(field.Name);
                Recurse(depth + 1, document, value.GetFieldValue(field.Name), nameBuilder);
                nameBuilder.Remove(name.Length, nameBuilder.Length - name.Length);
            }
        }
    }
}