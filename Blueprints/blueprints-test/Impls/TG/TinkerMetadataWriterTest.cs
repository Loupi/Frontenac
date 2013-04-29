using NUnit.Framework;
using System.IO;

namespace Frontenac.Blueprints.Impls.TG
{
    [TestFixture(Category = "TinkerMetadataWriterTest")]
    public class TinkerMetadataWriterTest
    {
        [Test]
        public void TestNormal()
        {
            TinkerGraph g = TinkerGraphFactory.CreateTinkerGraph();
            CreateManualIndices(g);
            CreateKeyIndices(g);

            using (var bos = new MemoryStream())
            {
                TinkerMetadataWriter.Save(g, bos);

                using (var stream = typeof(TinkerMetadataWriterTest).Assembly.GetManifestResourceStream(typeof(TinkerMetadataWriterTest), "example-tinkergraph-metadata.dat"))
                {
                    if (stream != null)
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
        }

        static void CreateKeyIndices(TinkerGraph g)
        {
            g.CreateKeyIndex("name", typeof(IVertex));
            g.CreateKeyIndex("weight", typeof(IEdge));
        }

        static void CreateManualIndices(TinkerGraph g)
        {
            IIndex idxAge = g.CreateIndex("age", typeof(IVertex));
            IVertex v1 = g.GetVertex(1);
            IVertex v2 = g.GetVertex(2);
            idxAge.Put("age", v1.GetProperty("age"), v1);
            idxAge.Put("age", v2.GetProperty("age"), v2);

            IIndex idxWeight = g.CreateIndex("weight", typeof(IEdge));
            IEdge e7 = g.GetEdge(7);
            IEdge e12 = g.GetEdge(12);
            idxWeight.Put("weight", e7.GetProperty("weight"), e7);
            idxWeight.Put("weight", e12.GetProperty("weight"), e12);
        }
    }
}
