using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Frontenac.Blueprints.Util.IO.GraphML
{
    /// <summary>
    /// http://www.cookcomputing.com/blog/archives/000577.html
    /// </summary>
    public class EncodingStreamWriter : StreamWriter
    {
        Encoding _encoding;

        public EncodingStreamWriter(Stream stm, Encoding encoding)
            : base(stm)
        {
            _encoding = encoding;
        }

        public override Encoding Encoding
        {
            get { return _encoding; }
        }
    }
}
