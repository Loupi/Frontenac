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
        public void testEdgeSetEquality()
        {
            Graph graph = TinkerGraphFactory.createTinkerGraph();

            foreach (Vertex v in graph.getVertices())
            {
                foreach (Vertex u in graph.getVertices())
                {
                    if (ElementHelper.areEqual(v, u))
                    {
                        Assert.True(VertexHelper.haveEqualEdges(v, u, true));
                        Assert.True(VertexHelper.haveEqualEdges(v, u, false));
                    }
                    else
                    {
                        Assert.False(VertexHelper.haveEqualEdges(v, u, true));
                        Assert.False(VertexHelper.haveEqualEdges(v, u, false));
                    }

                }
            }
        }

        [Test]
        public void testNeighborhoodEquality()
        {
            Graph graph = TinkerGraphFactory.createTinkerGraph();

            foreach (Vertex v in graph.getVertices())
            {
                foreach (Vertex u in graph.getVertices())
                {
                    if (ElementHelper.areEqual(v, u))
                    {
                        Assert.True(VertexHelper.haveEqualNeighborhood(v, u, true));
                        Assert.True(VertexHelper.haveEqualNeighborhood(v, u, false));
                    } else {
                        Assert.False(VertexHelper.haveEqualNeighborhood(v, u, true));
                        Assert.False(VertexHelper.haveEqualNeighborhood(v, u, false));
                    }
                }
            }
        }
    }
}
