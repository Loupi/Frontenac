using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Frontenac.Blueprints.Impls.TG;

namespace Frontenac.Blueprints.Util
{
    [TestFixture(Category = "VertexHelperTest")]
    public class VertexHelperTest : BaseTest
    {
        [Test]
        public void TestEdgeSetEquality()
        {
            Graph graph = TinkerGraphFactory.CreateTinkerGraph();

            foreach (Vertex v in graph.GetVertices())
            {
                foreach (Vertex u in graph.GetVertices())
                {
                    if (ElementHelper.AreEqual(v, u))
                    {
                        Assert.True(VertexHelper.HaveEqualEdges(v, u, true));
                        Assert.True(VertexHelper.HaveEqualEdges(v, u, false));
                    }
                    else
                    {
                        Assert.False(VertexHelper.HaveEqualEdges(v, u, true));
                        Assert.False(VertexHelper.HaveEqualEdges(v, u, false));
                    }

                }
            }
        }

        [Test]
        public void TestNeighborhoodEquality()
        {
            Graph graph = TinkerGraphFactory.CreateTinkerGraph();

            foreach (Vertex v in graph.GetVertices())
            {
                foreach (Vertex u in graph.GetVertices())
                {
                    if (ElementHelper.AreEqual(v, u))
                    {
                        Assert.True(VertexHelper.HaveEqualNeighborhood(v, u, true));
                        Assert.True(VertexHelper.HaveEqualNeighborhood(v, u, false));
                    } else {
                        Assert.False(VertexHelper.HaveEqualNeighborhood(v, u, true));
                        Assert.False(VertexHelper.HaveEqualNeighborhood(v, u, false));
                    }
                }
            }
        }
    }
}
