using System;

namespace Frontenac.Infrastructure.Indexing
{
    public abstract class QueryElement
    {
        private string _key;

        protected QueryElement(string key)
        {
            if (string.IsNullOrWhiteSpace(key))
                throw new ArgumentNullException(nameof(key));

            Key = key;
        }

        public string Key
        {
            get
            {
                return _key;
            }
            private set
            {
                if (value == null)
                    throw new ArgumentNullException(nameof(value));
                _key = value;
            }
        }
    }
}