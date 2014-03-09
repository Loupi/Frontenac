using System.Diagnostics.Contracts;

namespace Frontenac.Blueprints.Util.Wrappers.Batch.Cache
{
    [ContractClass(typeof (StringCompressionContract))]
    public abstract class StringCompression
    {
        public static StringCompression NoCompression = new NullStringCompression();

        public abstract string Compress(string input);

        private class NullStringCompression : StringCompression
        {
            public override string Compress(string input)
            {
                return input;
            }
        }
    }
}