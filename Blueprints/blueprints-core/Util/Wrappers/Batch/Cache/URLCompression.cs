﻿using System;
using System.Collections.Generic;
using System.Linq;

namespace Frontenac.Blueprints.Util.Wrappers.Batch.Cache
{
    public class UrlCompression : StringCompression
    {
        const string Delimiter = "$";
        static readonly char[] UrlDelimiters = new[] { '/', '#', ':' };

        int _prefixCounter;

        readonly Dictionary<string, string> _urlPrefix = new Dictionary<string, string>();

        public override string Compress(string input)
        {
            string[] url = SplitUrl(input);
            string prefix;
            _urlPrefix.TryGetValue(url[0], out prefix);
            if (prefix == null)
            {
                //New Prefix
                prefix = string.Concat(IntToBase36String(_prefixCounter), Delimiter);
                _prefixCounter++;
                _urlPrefix[url[0]] = prefix;
            }
            return prefix + url[1];
        }

        static string[] SplitUrl(string url)
        {
            var res = new string[2];
            int pos = UrlDelimiters.Select(delimiter => url.LastIndexOf(delimiter)).Concat(new[] {-1}).Max();
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
        static readonly char[] Base36Chars = new[] { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9', 'A', 'B', 'C', 'D', 'E', 'F', 'G', 'H', 'I', 'J', 'K', 'L', 'M', 'N', 'O', 'P', 'Q', 'R', 'S', 'T', 'U', 'V', 'W', 'X', 'Y', 'Z' };

        static string IntToBase36String(int value)
        {
            return IntToStringFast(value, Base36Chars);
        }

        /// <summary>
        /// http://stackoverflow.com/questions/923771/quickest-way-to-convert-a-base-10-number-to-any-base-in-net
        /// </summary>
        static string IntToStringFast(int value, char[] baseChars)
        {
            // 32 is the worst cast buffer size for base 2 and int.MaxValue
            int i = 32;
            var buffer = new char[i];
            int targetBase = baseChars.Length;

            do
            {
                buffer[--i] = baseChars[value % targetBase];
                value = value / targetBase;
            }
            while (value > 0);

            var result = new char[32 - i];
            Array.Copy(buffer, i, result, 0, 32 - i);

            return new string(result);
        }
    }
}
