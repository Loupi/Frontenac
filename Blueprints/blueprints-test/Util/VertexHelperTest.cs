using Frontenac.Blueprints.Impls.TG;
using NUnit.Framework;

namespace Frontenac.Blueprints.Util
{
    [TestFixture(Category = "VertexHelperTest")]
    public class VertexHelperTest : BaseTest
    {
        [Test]
        public void TestEdgeSetEquality()
        {
            var graph = TinkerGraphFactory.CreateTinkerGraph();

            foreach (var v in graph.GetVertices())
            {
                foreach (var u in graph.GetVertices())
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
            var graph = TinkerGraphFactory.CreateTinkerGraph();

            foreach (var v in graph.GetVertices())
            {
                foreach (var u in graph.GetVertices())
                {
                    if (ElementHelper.AreEqual(v, u))
                    {
                        Assert.True(VertexHelper.HaveEqualNeighborhood(v, u, true));
                        Assert.True(VertexHelper.HaveEqualNeighborhood(v, u, false));
                    }
                    else
                    {
                        Assert.False(VertexHelper.HaveEqualNeighborhood(v, u, true));
                        Assert.False(VertexHelper.HaveEqualNeighborhood(v, u, false));
                    }
                }
            }
        }
    }
}