using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Frontenac.Blueprints.Util.Wrappers.Batch.Cache
{
    public abstract class StringCompression
    {
        public static StringCompression NO_COMPRESSION = new NullStringCompression();

        class NullStringCompression : StringCompression
        {
            public override string compress(string input)
            {
                return input;
            }
        }

        public abstract string compress(string input);
    }
}
