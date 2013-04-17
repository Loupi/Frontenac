using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Frontenac.Blueprints.Impls;

namespace Frontenac.Blueprints
{
    public class TestSuite : BaseTest
    {
        readonly string _Name;
        protected GraphTest _GraphTest;

        [TestFixtureSetUp]
        public virtual void FixtureSetUp()
        {
            this.StopWatch();
        }

        [TestFixtureTearDown]
        public virtual void FixtureTearDown()
        {
            PrintTestPerformance(_Name, this.StopWatch());
        }

        public TestSuite(string name, GraphTest graphTest)
        {
            _Name = name;
            _GraphTest = graphTest;
        }

        protected string ConvertId(Graph graph, string id)
        {
            if (graph.GetFeatures().IsRDFModel.Value)
                return string.Concat("blueprints:", id);
            else
                return id;
        }

        protected void VertexCount(Graph graph, int expectedCount)
        {
            if (graph.GetFeatures().SupportsVertexIteration.Value)
                Assert.AreEqual(Count(graph.GetVertices()), expectedCount);
        }

        protected void ContainsVertices(Graph graph, IEnumerable<Vertex> vertices)
        {
            foreach (Vertex v in vertices)
            {
                Vertex vp = graph.GetVertex(v.GetId());
                if (vp == null || !vp.GetId().Equals(v.GetId())) Assert.Fail();
            }
        }

        protected void EdgeCount(Graph graph, int expectedCount)
        {
            if (graph.GetFeatures().SupportsEdgeIteration.Value)
                Assert.AreEqual(Count(graph.GetEdges()), expectedCount);
        }
    }
}
