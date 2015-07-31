using System.Linq;
using Frontenac.Blueprints.Impls.TG;
using NUnit.Framework;

namespace Frontenac.Blueprints.Util
{
    [TestFixture(Category = "KeyIndexableGraphHelperTest")]
    public class KeyIndexableGraphHelperTest : BaseTest
    {
        [Test]
        public void TestReIndexElements()
        {
            var graph = TinkerGraphFactory.CreateTinkerGraph();
            Assert.True(graph.GetVertices("name", "marko") is PropertyFilteredIterable<IVertex>);
            Assert.AreEqual(Count(graph.GetVertices("name", "marko")), 1);
            Assert.AreEqual(graph.GetVertices("name", "marko").First(), graph.GetVertex(1));
            graph.CreateKeyIndex("name", typeof (IVertex));
            Assert.False(graph.GetVertices("name", "marko") is PropertyFilteredIterable<IVertex>);
            Assert.AreEqual(Count(graph.GetVertices("name", "marko")), 1);
            Assert.AreEqual(graph.GetVertices("name", "marko").First(), graph.GetVertex(1));
        }
    }
}