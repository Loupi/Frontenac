using Frontenac.Blueprints.Impls.TG;
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
    }
}
