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
                    string expected = streamToByteArray(stream);
                    // ignore carriage return character...not really relevant to the test
                    Assert.AreEqual(expected.Replace("\r", ""), actual.Replace("\r", ""));
                }   
            }
        }

        [Test]
        public void testUseIds()
        {
            TinkerGraph g = new TinkerGraph();
            using(var stream = typeof(GMLReaderTest).Assembly.GetManifestResourceStream(typeof(GMLReaderTest), "example.gml"))
            {
                GMLReader.inputGraph(g, stream);
            }

            using(var stream = typeof(GMLReaderTest).Assembly.GetManifestResourceStream(typeof(GMLWriterTest), "writer2.gml"))
            {
                using (MemoryStream bos = new MemoryStream())
                {
                    GMLWriter w = new GMLWriter(g);
                    w.setNormalize(true);
                    w.setUseId(true);
                    w.outputGraph(bos);

                    bos.Position = 0;
                    string actual = new StreamReader(bos).ReadToEnd();
                    string expected = streamToByteArray(stream);

                    // ignore carriage return character...not really relevant to the test
                    Assert.AreEqual(expected.Replace("\r", ""), actual.Replace("\r", ""));
                }
            }
        }

        [Test]
        public void testRoundTrip()
        {
            TinkerGraph g1 = TinkerGraphFactory.createTinkerGraph();
            Graph g2 = new TinkerGraph();
            using (MemoryStream bos = new MemoryStream())
            {
                GMLWriter w = new GMLWriter(g1);
                w.setUseId(true);
                w.outputGraph(bos);
                bos.Position = 0;
                GMLReader.inputGraph(g2, bos);
            }
            
            Assert.AreEqual(g1.getVertices().Count(), g2.getVertices().Count());
            Assert.AreEqual(g1.getEdges().Count(), g2.getEdges().Count());
        }

        [Test]
        public void testRoundTripIgnoreBadProperties()
        {
            TinkerGraph g1 = TinkerGraphFactory.createTinkerGraph();
            Vertex v = g1.getVertex(1);
            v.setProperty("bad_property", "underscore");
            v.setProperty("bad property", "space");
            v.setProperty("bad-property", "dash");
            v.setProperty("bad$property", "other stuff");
            v.setProperty("badproperty_", "but don't get too fancy");
            v.setProperty("_badproperty", "or it won't work");
            v.setProperty("55", "numbers alone are bad");
            v.setProperty("5badproperty", "must start with alpha");
            v.setProperty("badpropertyajflalfjsajfdfjdkfjsdiahfshfksajdhfkjdhfkjhaskdfhaksdhfsalkjdfhkjdhkahsdfkjasdhfkajfdhkajfhkadhfkjsdafhkajfdhasdkfhakfdhakjsdfhkadhfakjfhaksdjhfkajfhakhfaksfdhkahdfkahfkajsdhfkjahdfkahsdfkjahfkhakfsdjhakjksfhakfhaksdhfkadhfakhfdkasfdhiuerfaeafdkjhakfdhfdadfasdfsdafadf", "super long keys won't work");
            v.setProperty("good5property", "numbers are cool");
            v.setProperty("goodproperty5", "numbers are cool");
            v.setProperty("a", "one letter is ok");

            
            Graph g2 = new TinkerGraph();
            using (MemoryStream bos = new MemoryStream())
            {
                GMLWriter w = new GMLWriter(g1);
                w.setStrict(true);
                w.setUseId(true);
                w.outputGraph(bos);
                bos.Position = 0;
                GMLReader.inputGraph(g2, bos);
            }
            
            Assert.AreEqual(g1.getVertices().Count(), g2.getVertices().Count());
            Assert.AreEqual(g1.getEdges().Count(), g2.getEdges().Count());

            Vertex v1 = g2.getVertex(1);
            Assert.Null(v1.getProperty("bad_property"));
            Assert.Null(v1.getProperty("bad property"));
            Assert.Null(v1.getProperty("bad-property"));
            Assert.Null(v1.getProperty("bad$property"));
            Assert.Null(v1.getProperty("_badproperty"));
            Assert.Null(v1.getProperty("badproperty_"));
            Assert.Null(v1.getProperty("5badproperty"));
            Assert.Null(v1.getProperty("55"));
            Assert.Null(v1.getProperty("badpropertyajflalfjsajfdfjdkfjsdiahfshfksajdhfkjdhfkjhaskdfhaksdhfsalkjdfhkjdhkahsdfkjasdhfkajfdhkajfhkadhfkjsdafhkajfdhasdkfhakfdhakjsdfhkadhfakjfhaksdjhfkajfhakhfaksfdhkahdfkahfkajsdhfkjahdfkahsdfkjahfkhakfsdjhakjksfhakfhaksdhfkadhfakhfdkasfdhiuerfaeafdkjhakfdhfdadfasdfsdafadf"));
            Assert.AreEqual("numbers are cool", v1.getProperty("good5property"));
            Assert.AreEqual("numbers are cool", v1.getProperty("goodproperty5"));
            Assert.AreEqual("one letter is ok", v1.getProperty("a"));
        }

        // Note: this is only a very lightweight test of writer/reader encoding.
        // It is known that there are characters which, when written by GMLWriter,
        // cause parse errors for GraphMLReader.
        // However, this happens uncommonly enough that is not yet known which characters those are.
        [Test]
        public void testEncoding(){

            Graph g = new TinkerGraph();
            Vertex v = g.addVertex(1);
            v.setProperty("text", "\u00E9");

            Graph g2 = new TinkerGraph();
            using (MemoryStream bos = new MemoryStream())
            {
                GMLWriter w = new GMLWriter(g);
                w.setStrict(true);
                w.setUseId(true);
                w.outputGraph(bos);
                bos.Position = 0;
                GMLReader r = new GMLReader(g2);
                r.inputGraph(bos);
            }

            Vertex v2 = g2.getVertex(1);
            Assert.AreEqual("\u00E9", v2.getProperty("text"));
        }

        string streamToByteArray(Stream in_)
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
