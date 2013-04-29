using NUnit.Framework;
using System.Collections.Generic;
using Frontenac.Blueprints.Impls;

namespace Frontenac.Blueprints
{
    public class TestSuite : BaseTest
    {
        readonly string _name;
        protected GraphTest GraphTest;

        [TestFixtureSetUp]
        public virtual void FixtureSetUp()
        {
            StopWatch();
        }

        [TestFixtureTearDown]
        public virtual void FixtureTearDown()
        {
            PrintTestPerformance(_name, StopWatch());
        }

        public TestSuite(string name, GraphTest graphTest)
        {
            _name = name;
            GraphTest = graphTest;
        }

        protected string ConvertId(IGraph graph, string id)
        {
            if (graph.GetFeatures().IsRdfModel)
                return string.Concat("blueprints:", id);
            return id;
        }

        protected void VertexCount(IGraph graph, int expectedCount)
        {
            if (graph.GetFeatures().SupportsVertexIteration)
                Assert.AreEqual(Count(graph.GetVertices()), expectedCount);
        }

        protected void ContainsVertices(IGraph graph, IEnumerable<IVertex> vertices)
        {
            foreach (IVertex v in vertices)
            {
                IVertex vp = graph.GetVertex(v.GetId());
                if (vp == null || !vp.GetId().Equals(v.GetId())) Assert.Fail();
            }
        }

        protected void EdgeCount(IGraph graph, int expectedCount)
        {
            if (graph.GetFeatures().SupportsEdgeIteration)
                Assert.AreEqual(Count(graph.GetEdges()), expectedCount);
        }
    }
}
