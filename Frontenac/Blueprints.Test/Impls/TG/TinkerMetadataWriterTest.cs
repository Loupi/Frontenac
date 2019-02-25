using System.IO;
using NUnit.Framework;

namespace Frontenac.Blueprints.Impls.TG
{
    [TestFixture(Category = "TinkerMetadataWriterTest")]
    public class TinkerMetadataWriterTest : BaseTest
    {
        private static void CreateKeyIndices(TinkerGrapĥ g)
        {
            g.CreateKeyIndex("name", typeof (IVertex));
            g.CreateKeyIndex("weight", typeof (IEdge));
        }

        private static void CreateManualIndices(TinkerGrapĥ g)
        {
            var idxAge = g.CreateIndex("age", typeof (IVertex));
            var v1 = g.GetVertex(1);
            var v2 = g.GetVertex(2);
            idxAge.Put("age", v1.GetProperty("age"), v1);
            idxAge.Put("age", v2.GetProperty("age"), v2);

            var idxWeight = g.CreateIndex("weight", typeof (IEdge));
            var e7 = g.GetEdge(7);
            var e12 = g.GetEdge(12);
            idxWeight.Put("weight", e7.GetProperty("weight"), e7);
            idxWeight.Put("weight", e12.GetProperty("weight"), e12);
        }

        [Test]
        public void TestNormal()
        {
            var g = TinkerGraphFactory.CreateTinkerGraph();
            CreateManualIndices(g);
            CreateKeyIndices(g);

            using (var bos = new MemoryStream())
            {
                TinkerMetadataWriter.Save(g, bos);
                using (var stream = GetResource<TinkerMetadataWriterTest>("example-tinkergraph-metadata.dat"))
                {
                    if (stream == null) return;

                    var ms = new MemoryStream((int)stream.Length);
                    stream.CopyTo(ms);

                    var expected = ms.ToArray();
                    var actual = bos.ToArray();




                    Assert.AreEqual(expected.Length, actual.Length);

                    for (var ix = 0; ix < actual.Length; ix++)
                    {
                        Assert.AreEqual(expected[ix], actual[ix]);
                    }
                }
            }
        }
    }
}