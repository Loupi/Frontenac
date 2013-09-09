﻿using NUnit.Framework;
using System.IO;
using System.Linq;
using System.Text;
using Frontenac.Blueprints.Impls.TG;

namespace Frontenac.Blueprints.Util.IO.GML
{
    [TestFixture(Category = "GMLWriterTest")]
    public class GmlWriterTest
    {
        [Test]
        public void TestNormal()
        {
            var g = new TinkerGraph();
            using(var stream = typeof(GmlReaderTest).Assembly.GetManifestResourceStream(typeof(GmlReaderTest), "example.gml"))
            {
                GmlReader.InputGraph(g, stream);
            }

            using(var bos = new MemoryStream())
            {
                var w = new GmlWriter(g);
                w.Normalize = true;
                w.OutputGraph(bos);

                string actual = Encoding.GetEncoding("ISO-8859-1").GetString(bos.ToArray());
                using(var stream = typeof(GmlWriterTest).Assembly.GetManifestResourceStream(typeof(GmlReaderTest), "writer.gml"))
                {
                    string expected = StreamToByteArray(stream);
                    // ignore carriage return character...not really relevant to the test
                    Assert.AreEqual(expected.Replace("\r", ""), actual.Replace("\r", ""));
                }   
            }
        }

        [Test]
        public void TestUseIds()
        {
            var g = new TinkerGraph();
            using(var stream = typeof(GmlReaderTest).Assembly.GetManifestResourceStream(typeof(GmlReaderTest), "example.gml"))
            {
                GmlReader.InputGraph(g, stream);
            }

            using(var stream = typeof(GmlReaderTest).Assembly.GetManifestResourceStream(typeof(GmlWriterTest), "writer2.gml"))
            {
                using (var bos = new MemoryStream())
                {
                    var w = new GmlWriter(g);
                    w.Normalize = true;
                    w.UseId = true;
                    w.OutputGraph(bos);

                    bos.Position = 0;
                    string actual = new StreamReader(bos).ReadToEnd();
                    string expected = StreamToByteArray(stream);

                    // ignore carriage return character...not really relevant to the test
                    Assert.AreEqual(expected.Replace("\r", ""), actual.Replace("\r", ""));
                }
            }
        }

        [Test]
        public void TestRoundTrip()
        {
            TinkerGraph g1 = TinkerGraphFactory.CreateTinkerGraph();
            IGraph g2 = new TinkerGraph();
            using (var bos = new MemoryStream())
            {
                var w = new GmlWriter(g1);
                w.UseId = true;
                w.OutputGraph(bos);
                bos.Position = 0;
                GmlReader.InputGraph(g2, bos);
            }
            
            Assert.AreEqual(g1.GetVertices().Count(), g2.GetVertices().Count());
            Assert.AreEqual(g1.GetEdges().Count(), g2.GetEdges().Count());
        }

        [Test]
        public void TestRoundTripIgnoreBadProperties()
        {
            TinkerGraph g1 = TinkerGraphFactory.CreateTinkerGraph();
            IVertex v = g1.GetVertex(1);
            v.SetProperty("bad_property", "underscore");
            v.SetProperty("bad property", "space");
            v.SetProperty("bad-property", "dash");
            v.SetProperty("bad$property", "other stuff");
            v.SetProperty("badproperty_", "but don't get too fancy");
            v.SetProperty("_badproperty", "or it won't work");
            v.SetProperty("55", "numbers alone are bad");
            v.SetProperty("5badproperty", "must start with alpha");
            v.SetProperty("badpropertyajflalfjsajfdfjdkfjsdiahfshfksajdhfkjdhfkjhaskdfhaksdhfsalkjdfhkjdhkahsdfkjasdhfkajfdhkajfhkadhfkjsdafhkajfdhasdkfhakfdhakjsdfhkadhfakjfhaksdjhfkajfhakhfaksfdhkahdfkahfkajsdhfkjahdfkahsdfkjahfkhakfsdjhakjksfhakfhaksdhfkadhfakhfdkasfdhiuerfaeafdkjhakfdhfdadfasdfsdafadf", "super long keys won't work");
            v.SetProperty("good5property", "numbers are cool");
            v.SetProperty("goodproperty5", "numbers are cool");
            v.SetProperty("a", "one letter is ok");

            
            IGraph g2 = new TinkerGraph();
            using (var bos = new MemoryStream())
            {
                var w = new GmlWriter(g1);
                w.Strict = true;
                w.UseId = true;
                w.OutputGraph(bos);
                bos.Position = 0;
                GmlReader.InputGraph(g2, bos);
            }
            
            Assert.AreEqual(g1.GetVertices().Count(), g2.GetVertices().Count());
            Assert.AreEqual(g1.GetEdges().Count(), g2.GetEdges().Count());

            IVertex v1 = g2.GetVertex(1);
            Assert.Null(v1.GetProperty("bad_property"));
            Assert.Null(v1.GetProperty("bad property"));
            Assert.Null(v1.GetProperty("bad-property"));
            Assert.Null(v1.GetProperty("bad$property"));
            Assert.Null(v1.GetProperty("_badproperty"));
            Assert.Null(v1.GetProperty("badproperty_"));
            Assert.Null(v1.GetProperty("5badproperty"));
            Assert.Null(v1.GetProperty("55"));
            Assert.Null(v1.GetProperty("badpropertyajflalfjsajfdfjdkfjsdiahfshfksajdhfkjdhfkjhaskdfhaksdhfsalkjdfhkjdhkahsdfkjasdhfkajfdhkajfhkadhfkjsdafhkajfdhasdkfhakfdhakjsdfhkadhfakjfhaksdjhfkajfhakhfaksfdhkahdfkahfkajsdhfkjahdfkahsdfkjahfkhakfsdjhakjksfhakfhaksdhfkadhfakhfdkasfdhiuerfaeafdkjhakfdhfdadfasdfsdafadf"));
            Assert.AreEqual("numbers are cool", v1.GetProperty("good5property"));
            Assert.AreEqual("numbers are cool", v1.GetProperty("goodproperty5"));
            Assert.AreEqual("one letter is ok", v1.GetProperty("a"));
        }

        // Note: this is only a very lightweight test of writer/reader encoding.
        // It is known that there are characters which, when written by GMLWriter,
        // cause parse errors for GraphMLReader.
        // However, this happens uncommonly enough that is not yet known which characters those are.
        [Test]
        public void TestEncoding(){

            IGraph g = new TinkerGraph();
            IVertex v = g.AddVertex(1);
            v.SetProperty("text", "\u00E9");

            IGraph g2 = new TinkerGraph();
            using (var bos = new MemoryStream())
            {
                var w = new GmlWriter(g);
                w.Strict = true;
                w.UseId = true;
                w.OutputGraph(bos);
                bos.Position = 0;
                var r = new GmlReader(g2);
                r.InputGraph(bos);
            }

            IVertex v2 = g2.GetVertex(1);
            Assert.AreEqual("\u00E9", v2.GetProperty("text"));
        }

        static string StreamToByteArray(Stream in_)
        {
            using(var buffer = new MemoryStream())
            {
                try
                {
                    int nRead;
                    var data = new byte[1024];

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
