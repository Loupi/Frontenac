using System.IO;
using Frontenac.Blueprints.Impls.TG;
using NUnit.Framework;

namespace Frontenac.Blueprints.Util.IO.GraphML
{
    [TestFixture(Category = "GraphMLWriterTest")]
    public class GraphMlWriterTest : BaseTest
    {
        [Test]
        public void TestEncoding()
        {
            var g = new TinkerGraph();
            var v = g.AddVertex(1);
            v.SetProperty("text", "\u00E9");

            var g2 = new TinkerGraph();
            using (var bos = new MemoryStream())
            {
                var w = new GraphMlWriter(g);
                w.OutputGraph(bos);
                bos.Position = 0;
                var r = new GraphMlReader(g2);
                r.InputGraph(bos);
            }

            var v2 = g2.GetVertex(1);
            Assert.AreEqual("\u00E9", v2.GetProperty("text"));
        }

        [Test]
        public void TestNormal()
        {
            var g = new TinkerGraph();
            using (var stream = GetResource<GraphMlReader>("graph-example-1.xml"))
            {
                GraphMlReader.InputGraph(g, stream);
            }

            using (var bos = new MemoryStream())
            {
                var w = new GraphMlWriter(g);
                w.SetNormalize(true);
                w.OutputGraph(bos);
                bos.Position = 0;
                var outGraphMl = new StreamReader(bos).ReadToEnd();

                using (var stream = GetResource<GraphMlWriterTest>("graph-example-1-normalized.xml"))
                {
                    if (stream != null)
                    {
                        var expected = new StreamReader(stream).ReadToEnd();
                        Assert.AreEqual(expected.Replace("\n", "").Replace("\r", "").Replace("\t", ""),
                                        outGraphMl.Replace("\n", "").Replace("\r", "").Replace("\t", ""));
                    }
                }
            }
        }

        [Test]
        public void TestWithEdgeLabel()
        {
            var g = new TinkerGraph();

            using (var stream = GetResource<GraphMlReader>("graph-example-1.xml"))
            {
                GraphMlReader.InputGraph(g, stream);
            }

            using (var bos = new MemoryStream())
            {
                var w = new GraphMlWriter(g);
                w.SetEdgeLabelKey("label");
                w.SetNormalize(true);
                w.OutputGraph(bos);
                bos.Position = 0;
                var outGraphMl = new StreamReader(bos).ReadToEnd();

                using (var stream = GetResource<GraphMlWriterTest>("graph-example-1-schema-valid.xml"))
                {
                    if (stream != null)
                    {
                        var expected = new StreamReader(stream).ReadToEnd();
                        Assert.AreEqual(expected.Replace("\n", "").Replace("\r", ""),
                                        outGraphMl.Replace("\n", "").Replace("\r", ""));
                    }
                }
            }
        }

        // Note: this is only a very lightweight test of writer/reader encoding.
        // It is known that there are characters which, when written by GraphMLWriter,
        // cause parse errors for GraphMLReader.
        // However, this happens uncommonly enough that is not yet known which characters those are.
    }
}