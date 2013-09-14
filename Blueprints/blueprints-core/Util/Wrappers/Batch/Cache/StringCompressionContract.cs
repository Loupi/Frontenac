using System.Diagnostics.Contracts;

namespace Frontenac.Blueprints.Util.Wrappers.Batch.Cache
{
    [ContractClassFor(typeof (StringCompression))]
    public abstract class StringCompressionContract : StringCompression
    {
        public override string Compress(string input)
        {
            Contract.Requires(input != null);
            Contract.Ensures(Contract.Result<string>() != null);
            return null;
        }
    }
}