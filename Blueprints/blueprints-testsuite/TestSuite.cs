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
        readonly string _name;
        protected GraphTest graphTest;

        [TestFixtureSetUp]
        public virtual void fixtureSetUp()
        {
            this.stopWatch();
        }

        [TestFixtureTearDown]
        public virtual void fixtureTearDown()
        {
            printTestPerformance(_name, this.stopWatch());
        }

        public TestSuite(string name, GraphTest graphTest)
        {
            _name = name;
            this.graphTest = graphTest;
        }

        protected string convertId(Graph graph, string id)
        {
            if (graph.getFeatures().isRDFModel.Value)
                return string.Concat("blueprints:", id);
            else
                return id;
        }

        protected void vertexCount(Graph graph, int expectedCount)
        {
            if (graph.getFeatures().supportsVertexIteration.Value)
                Assert.AreEqual(count(graph.getVertices()), expectedCount);
        }

        protected void containsVertices(Graph graph, IEnumerable<Vertex> vertices)
        {
            foreach (Vertex v in vertices)
            {
                Vertex vp = graph.getVertex(v.getId());
                if (vp == null || !vp.getId().Equals(v.getId())) Assert.Fail();
            }
        }

        protected void edgeCount(Graph graph, int expectedCount)
        {
            if (graph.getFeatures().supportsEdgeIteration.Value)
                Assert.AreEqual(count(graph.getEdges()), expectedCount);
        }
    }
}
