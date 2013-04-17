using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Frontenac.Blueprints.Util.Wrappers.Batch.Cache
{
    public class URLCompression : StringCompression
    {
        const string DELIMITER = "$";
        static readonly char[] urlDelimiters = new char[] { '/', '#', ':' };

        int _PrefixCounter = 0;

        readonly Dictionary<string, string> _UrlPrefix = new Dictionary<string, string>();

        public override string Compress(string input)
        {
            string[] url = SplitURL(input);
            string prefix = null;
            _UrlPrefix.TryGetValue(url[0], out prefix);
            if (prefix == null)
            {
                //New Prefix
                prefix = string.Concat(IntToBase36String(_PrefixCounter) + DELIMITER);
                _PrefixCounter++;
                _UrlPrefix[url[0]] = prefix;
            }
            return prefix + url[1];
        }

        static string[] SplitURL(string url)
        {
            string[] res = new string[2];
            int pos = -1;
            foreach (char delimiter in urlDelimiters)
            {
                int currentpos = url.LastIndexOf(delimiter);
                if (currentpos > pos) pos = currentpos;
            }
            if (pos < 0)
            {
                res[0] = "";
                res[1] = url;
            }
            else
            {
                res[0] = url.Substring(0, pos + 1);
                res[1] = url.Substring(pos + 1);
            }
            return res;
        }

        //see http://msdn.microsoft.com/en-ca/library/aa245218%28v=vs.60%29.aspx and
        //http://grepcode.com/file/repository.grepcode.com/java/root/jdk/openjdk/6-b14/java/lang/Character.java
        //where I found out that MAX_RADIX = 36

        //const int MAX_RADIX = 36;
        static readonly char[] BASE36_CHARS = new char[] { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9', 'A', 'B', 'C', 'D', 'E', 'F', 'G', 'H', 'I', 'J', 'K', 'L', 'M', 'N', 'O', 'P', 'Q', 'R', 'S', 'T', 'U', 'V', 'W', 'X', 'Y', 'Z' };

        static string IntToBase36String(int value)
        {
            return IntToStringFast(value, BASE36_CHARS);
        }

        /// <summary>
        /// http://stackoverflow.com/questions/923771/quickest-way-to-convert-a-base-10-number-to-any-base-in-net
        /// </summary>
        static string IntToStringFast(int value, char[] baseChars)
        {
            // 32 is the worst cast buffer size for base 2 and int.MaxValue
            int i = 32;
            char[] buffer = new char[i];
            int targetBase = baseChars.Length;

            do
            {
                buffer[--i] = baseChars[value % targetBase];
                value = value / targetBase;
            }
            while (value > 0);

            char[] result = new char[32 - i];
            Array.Copy(buffer, i, result, 0, 32 - i);

            return new string(result);
        }
    }
}
