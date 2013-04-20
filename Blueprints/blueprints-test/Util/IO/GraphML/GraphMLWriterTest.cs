using Frontenac.Blueprints.Impls.TG;
using Frontenac.Blueprints.Util.IO.GML;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Frontenac.Blueprints.Util.IO.GraphML
{
    [TestFixture(Category = "GraphMLWriterTest")]
    public class GraphMLWriterTest
    {
        [Test]
        public void testNormal()
        {
            TinkerGraph g = new TinkerGraph();

            using (var stream = typeof(GraphMLReader).Assembly.GetManifestResourceStream(typeof(GraphMLReader),
                                                                                           "graph-example-1.xml"))
            {
                GraphMLReader.inputGraph(g, stream);
            }

            using (var bos = new MemoryStream())
            {
                GraphMLWriter w = new GraphMLWriter(g);
                w.setNormalize(true);
                w.outputGraph(bos);
                bos.Position = 0;
                string outGraphML = new StreamReader(bos).ReadToEnd();

                using (var stream = typeof (GraphMLWriterTest).Assembly.GetManifestResourceStream(typeof (GraphMLWriterTest),
                                                                                           "graph-example-1-normalized.xml"))
                {
                    string expected = new StreamReader(stream).ReadToEnd();
                    Assert.AreEqual(expected.Replace("\n", "").Replace("\r", ""), outGraphML.Replace("\n", "").Replace("\r", ""));
                }
            }
        }

        [Test]
        public void testWithEdgeLabel()
        {
            TinkerGraph g = new TinkerGraph();
            
            using (var stream = typeof(GraphMLReader).Assembly.GetManifestResourceStream(typeof(GraphMLReader),
                                                                                           "graph-example-1.xml"))
            {
                GraphMLReader.inputGraph(g, stream);
            }

            using (var bos = new MemoryStream())
            {
                GraphMLWriter w = new GraphMLWriter(g);
                w.setEdgeLabelKey("label");
                w.setNormalize(true);
                w.outputGraph(bos);
                bos.Position = 0;
                string outGraphML = new StreamReader(bos).ReadToEnd();

                using (var stream = typeof (GraphMLWriterTest).Assembly.GetManifestResourceStream(typeof (GraphMLWriterTest),
                                                                                           "graph-example-1-schema-valid.xml"))
                {
                    string expected = new StreamReader(stream).ReadToEnd();
                    Assert.AreEqual(expected.Replace("\n", "").Replace("\r", ""), outGraphML.Replace("\n", "").Replace("\r", ""));
                }
            }
        }

        // Note: this is only a very lightweight test of writer/reader encoding.
        // It is known that there are characters which, when written by GraphMLWriter,
        // cause parse errors for GraphMLReader.
        // However, this happens uncommonly enough that is not yet known which characters those are.
        [Test]
        public void testEncoding()
        {
            Graph g = new TinkerGraph();
            Vertex v = g.addVertex(1);
            v.setProperty("text", "\u00E9");

            Graph g2 = new TinkerGraph();
            using (MemoryStream bos = new MemoryStream())
            {
                GraphMLWriter w = new GraphMLWriter(g);
                w.outputGraph(bos);
                bos.Position = 0;
                GraphMLReader r = new GraphMLReader(g2);
                r.inputGraph(bos);
            }

            Vertex v2 = g2.getVertex(1);
            Assert.AreEqual("\u00E9", v2.getProperty("text"));
        }
    }
}
