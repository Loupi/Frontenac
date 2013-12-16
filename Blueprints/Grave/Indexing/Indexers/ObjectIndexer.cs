using System.Collections;
using System.Diagnostics.Contracts;
using System.Security.Principal;
using System.Text;
using Fasterflect;

namespace Frontenac.Grave.Indexing.Indexers
{
    public class ObjectIndexer : Indexer
    {
        private readonly object _content;
        private readonly IIndexerFactory _indexerFactory;
        private readonly uint _maxDepth;
        private readonly StringBuilder _nameBuilder = new StringBuilder();

        public ObjectIndexer(object content, IDocument document, IIndexerFactory indexerFactory, uint maxDepth)
            : base(document)
        {
            Contract.Requires(content != null);
            Contract.Requires(indexerFactory != null);
            Contract.Requires(maxDepth > 0);

            _content = content;
            _indexerFactory = indexerFactory;
            _maxDepth = maxDepth;
        }

        public override void Index(string name)
        {
            _nameBuilder.Append(name);
            Recurse(0, _content);
        }

        private void Recurse(uint depth, object value)
        {
            var type = value.GetType();

            if (!type.IsValueType && Document.Present(value))
                return;

            var name = _nameBuilder.ToString();

            if (Document.Write(name, value))
                return;

            var indexer = _indexerFactory.Create(value, Document);
            if (!(indexer is ObjectIndexer))
            {
                indexer.Index(name);
                return;
            }

            if ((type.IsValueType && type.FullName == string.Concat("System.", type.Name)) ||
                (type.FullName == string.Concat("System.Reflection.", type.Name)) ||
                (value is SecurityIdentifier))
                return;

            if (value is IEnumerable)
            {
                foreach (var obj in value as IEnumerable)
                {
                    Recurse(depth + 1, obj);
                }
                return;
            }

            var properties = type.Properties(Flags.InstancePublic);
            var fields = type.Fields(Flags.InstancePublic);

            if (properties.Count == 0 && fields.Count == 0)
            {
                var stringValue = value.ToString();
                if (stringValue != type.FullName)
                    Document.Write(name, stringValue);
            }

            if (depth >= _maxDepth) return;

            foreach (var property in properties)
            {
                if (_nameBuilder.Length > 0)
                    _nameBuilder.Append(".");
                _nameBuilder.Append(property.Name);
                Recurse(depth + 1, value.GetPropertyValue(property.Name));
                _nameBuilder.Remove(name.Length, _nameBuilder.Length - name.Length);
            }

            foreach (var field in fields)
            {
                if (_nameBuilder.Length > 0)
                    _nameBuilder.Append(".");
                _nameBuilder.Append(field.Name);
                Recurse(depth + 1, value.GetFieldValue(field.Name));
                _nameBuilder.Remove(name.Length, _nameBuilder.Length - name.Length);
            }
        }
    }
}