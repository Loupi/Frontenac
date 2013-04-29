using NUnit.Framework;
using Frontenac.Blueprints.Impls.TG;

namespace Frontenac.Blueprints.Util
{
    [TestFixture(Category = "VertexHelperTest")]
    public class VertexHelperTest : BaseTest
    {
        [Test]
        public void TestEdgeSetEquality()
        {
            IGraph graph = TinkerGraphFactory.CreateTinkerGraph();

            foreach (IVertex v in graph.GetVertices())
            {
                foreach (IVertex u in graph.GetVertices())
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
            IGraph graph = TinkerGraphFactory.CreateTinkerGraph();

            foreach (IVertex v in graph.GetVertices())
            {
                foreach (IVertex u in graph.GetVertices())
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
