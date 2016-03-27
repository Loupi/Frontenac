using System.IO;
using System.Text;
using Frontenac.Blueprints.Impls.TG;
using NUnit.Framework;

namespace Frontenac.Blueprints.Util.IO.GraphJson
{
    [TestFixture(Category = "GraphJsonTest")]
    public class GraphJsonTest : BaseTest
    {
        [Test]
        public void WriteTinkerGraph()
        {
            var g = TinkerGraphFactory.CreateTinkerGraph();

            using (var stream = new MemoryStream())
            {
                GraphJsonWriter.OutputGraph(g, stream);

                stream.Position = 0;
                var jsonString = Encoding.Default.GetString(stream.ToArray());
                using (var example = GetResource<GraphJsonTest>("graph-example-1.json"))
                {
                    var sr = new StreamReader(example);
                    Assert.AreEqual(jsonString, sr.ReadToEnd());
                }
            }
        }

        [Test]
        public void ReadTinkerGraph()
        {
            var g = new TinkerGrapĥ();

            using (var example = GetResource<GraphJsonTest>("graph-example-1.json"))
            {
                GraphJsonReader.InputGraph(g, example);
            }

            var marko = g.GetVertex("1");
            Assert.NotNull(marko);

            var name = marko.GetProperty("name");
            Assert.AreEqual("marko", name);
        }

        [Test]
        public void ReadCharlizGraph()
        {
            var g = new TinkerGrapĥ();

            using (var example = GetResource<GraphJsonTest>("Charliz.json"))
            {
                GraphJsonReader.InputGraph(g, example);
            }

            var award = g.GetVertex(595206);
            Assert.NotNull(award);

            var type = award.GetProperty("type");
            Assert.AreEqual("award", type);
        }
    }
}
