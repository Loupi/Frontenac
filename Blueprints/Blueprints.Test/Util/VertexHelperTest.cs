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
                    if (v.AreEqual(u))
                    {
                        Assert.True(v.HaveEqualEdges(u, true));
                        Assert.True(v.HaveEqualEdges(u, false));
                    }
                    else
                    {
                        Assert.False(v.HaveEqualEdges(u, true));
                        Assert.False(v.HaveEqualEdges(u, false));
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
                    if (v.AreEqual(u))
                    {
                        Assert.True(v.HaveEqualNeighborhood(u, true));
                        Assert.True(v.HaveEqualNeighborhood(u, false));
                    }
                    else
                    {
                        Assert.False(v.HaveEqualNeighborhood(u, true));
                        Assert.False(v.HaveEqualNeighborhood(u, false));
                    }
                }
            }
        }
    }
}