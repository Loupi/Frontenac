using System.Collections.Generic;
using Frontenac.Blueprints.Impls;
using NUnit.Framework;

namespace Frontenac.Blueprints
{
    public class TestSuite : BaseTest
    {
        private readonly string _name;
        protected GraphTest GraphTest;

        public TestSuite(string name, GraphTest graphTest)
        {
            _name = name;
            GraphTest = graphTest;
        }

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

        protected string ConvertId(IGraph graph, string id)
        {
            if (graph.Features.IsRdfModel)
                return string.Concat("blueprints:", id);
            return id;
        }

        protected void VertexCount(IGraph graph, int expectedCount)
        {
            if (graph.Features.SupportsVertexIteration)
                Assert.AreEqual(Count(graph.GetVertices()), expectedCount);
        }

        protected void ContainsVertices(IGraph graph, IEnumerable<IVertex> vertices)
        {
            foreach (var v in vertices)
            {
                var vp = graph.GetVertex(v.Id);
                if (vp == null || !vp.Id.Equals(v.Id)) Assert.Fail();
            }
        }

        protected void EdgeCount(IGraph graph, int expectedCount)
        {
            if (graph.Features.SupportsEdgeIteration)
                Assert.AreEqual(Count(graph.GetEdges()), expectedCount);
        }
    }
}