using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Frontenac.Blueprints.Impls.TG;

namespace Frontenac.Blueprints.Util
{
    [TestFixture(Category = "KeyIndexableGraphHelperTest")]
    public class KeyIndexableGraphHelperTest : BaseTest
    {
        [Test]
        public void testReIndexElements()
        {
            TinkerGraph graph = TinkerGraphFactory.createTinkerGraph();
            Assert.True(graph.getVertices("name", "marko") is PropertyFilteredIterable<Vertex>);
            Assert.AreEqual(count(graph.getVertices("name", "marko")), 1);
            Assert.AreEqual(graph.getVertices("name", "marko").First(), graph.getVertex(1));
            graph.createKeyIndex("name", typeof(Vertex));
            //KeyIndexableGraphHelper.reIndexElements(graph, graph.getVertices(), new HashSet<string>(Arrays.asList("name")));
            Assert.False(graph.getVertices("name", "marko") is PropertyFilteredIterable<Vertex>);
            Assert.AreEqual(count(graph.getVertices("name", "marko")), 1);
            Assert.AreEqual(graph.getVertices("name", "marko").First(), graph.getVertex(1));
        }
    }
}
