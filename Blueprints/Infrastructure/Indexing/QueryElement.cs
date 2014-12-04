using System.Diagnostics.Contracts;

namespace Frontenac.Infrastructure.Indexing
{
    public abstract class QueryElement
    {
        private string _key;

        protected QueryElement(string key)
        {
            Contract.Requires(!string.IsNullOrWhiteSpace(key));

            Key = key;
        }

        public string Key
        {
            get
            {
                Contract.Ensures(Contract.Result<string>() != null);
                return _key;
            }
            private set
            {
                Contract.Requires(value != null);
                _key = value;
            }
        }
    }
}