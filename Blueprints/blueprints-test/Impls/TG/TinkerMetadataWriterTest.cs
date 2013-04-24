using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Frontenac.Blueprints.Impls.TG
{
    [TestFixture(Category = "TinkerMetadataWriterTest")]
    public class TinkerMetadataWriterTest
    {
        [Test]
        public void testNormal()
        {
            TinkerGraph g = TinkerGraphFactory.createTinkerGraph();
            createManualIndices(g);
            createKeyIndices(g);

            using (var bos = new MemoryStream())
            {
                TinkerMetadataWriter.save(g, bos);

                using (var stream = typeof(TinkerMetadataWriterTest).Assembly.GetManifestResourceStream(typeof(TinkerMetadataWriterTest), "example-tinkergraph-metadata.dat"))
                {
                    var ms = new MemoryStream((int)stream.Length);
                    stream.CopyTo(ms);

                    byte[] expected = ms.ToArray();
                    byte[] actual = bos.ToArray();

                    Assert.AreEqual(expected.Length, actual.Length);

                    for (int ix = 0; ix < actual.Length; ix++)
                    {
                        Assert.AreEqual(expected[ix], actual[ix]);
                    }
                }
            }
        }

        void createKeyIndices(TinkerGraph g)
        {
            g.createKeyIndex("name", typeof(Vertex));
            g.createKeyIndex("weight", typeof(Edge));
        }

        void createManualIndices(TinkerGraph g)
        {
            Index idxAge = g.createIndex("age", typeof(Vertex));
            Vertex v1 = g.getVertex(1);
            Vertex v2 = g.getVertex(2);
            idxAge.put("age", v1.getProperty("age"), v1);
            idxAge.put("age", v2.getProperty("age"), v2);

            Index idxWeight = g.createIndex("weight", typeof(Edge));
            Edge e7 = g.getEdge(7);
            Edge e12 = g.getEdge(12);
            idxWeight.put("weight", e7.getProperty("weight"), e7);
            idxWeight.put("weight", e12.getProperty("weight"), e12);
        }
    }
}
