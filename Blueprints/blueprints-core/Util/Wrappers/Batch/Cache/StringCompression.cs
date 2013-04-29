namespace Frontenac.Blueprints.Util.Wrappers.Batch.Cache
{
    public abstract class StringCompression
    {
        public static StringCompression NoCompression = new NullStringCompression();

        class NullStringCompression : StringCompression
        {
            public override string Compress(string input)
            {
                return input;
            }
        }

        public abstract string Compress(string input);
    }
}
