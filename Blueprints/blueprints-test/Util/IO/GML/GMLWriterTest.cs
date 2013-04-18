using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Frontenac.Blueprints.Impls.TG;

namespace Frontenac.Blueprints.Util.IO.GML
{
    [TestFixture(Category = "GMLWriterTest")]
    public class GMLWriterTest
    {
        [Test]
        public void testNormal()
        {
            TinkerGraph g = new TinkerGraph();
            using(var stream = typeof(GMLReaderTest).Assembly.GetManifestResourceStream(typeof(GMLReaderTest), "example.gml"))
            {
                GMLReader.inputGraph(g, stream);
            }

            using(MemoryStream bos = new MemoryStream())
            {
                GMLWriter w = new GMLWriter(g);
                w.setNormalize(true);
                w.outputGraph(bos);

                string actual = Encoding.GetEncoding("ISO-8859-1").GetString(bos.ToArray());
                using(var stream = typeof(GMLWriterTest).Assembly.GetManifestResourceStream(typeof(GMLReaderTest), "writer.gml"))
                {
                    string expected = StreamToByteArray(stream);
                    // ignore carriage return character...not really relevant to the test
                    Assert.AreEqual(expected.Replace("\r", ""), actual.Replace("\r", ""));
                }   
            }
        }

        string StreamToByteArray(Stream in_)
        {
            using(MemoryStream buffer = new MemoryStream())
            {
                try
                {
                    int nRead;
                    byte[] data = new byte[1024];

                    while ((nRead = in_.Read(data, 0, data.Length)) > 0)
                        buffer.Write(data, 0, nRead);

                    buffer.Flush();
                }
                finally
                {
                    buffer.Close();
                }
                return Encoding.GetEncoding("ISO-8859-1").GetString(buffer.ToArray());
            }
        }
    }
}
