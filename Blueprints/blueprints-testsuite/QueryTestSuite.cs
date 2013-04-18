using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Frontenac.Blueprints.Impls;

namespace Frontenac.Blueprints
{
    public abstract class QueryTestSuite : TestSuite
    {
        public QueryTestSuite(GraphTest graphTest)
            : base("QueryTestSuite", graphTest)
        {

        }

        [Test]
        public void testVertexQuery()
        {
            Graph graph = graphTest.generateGraph();
            if (graph.getFeatures().supportsEdgeProperties.Value)
            {

                Vertex a = graph.addVertex(null);
                Vertex b = graph.addVertex(null);
                Vertex c = graph.addVertex(null);
                Edge aFriendB = graph.addEdge(null, a, b, convertId(graph, "friend"));
                Edge aFriendC = graph.addEdge(null, a, c, convertId(graph, "friend"));
                Edge aHateC = graph.addEdge(null, a, c, convertId(graph, "hate"));
                Edge cHateA = graph.addEdge(null, c, a, convertId(graph, "hate"));
                Edge cHateB = graph.addEdge(null, c, b, convertId(graph, "hate"));
                aFriendB.setProperty("amount", 1.0);
                aFriendB.setProperty("date", 10);
                aFriendC.setProperty("amount", 0.5);
                aHateC.setProperty("amount", 1.0);
                cHateA.setProperty("amount", 1.0);
                cHateB.setProperty("amount", 0.4);

                Assert.AreEqual(count(a.query().labels("friend").has("date", null).edges()), 1);
                Assert.AreEqual(a.query().labels("friend").has("date", null).edges().First().getProperty("amount"), 0.5);

                // out Edges

                IEnumerable<object> results = a.query().direction(Direction.OUT).edges();
                Assert.AreEqual(results.Count(), 3);
                Assert.True(results.Contains(aFriendB));
                Assert.True(results.Contains(aFriendC));
                Assert.True(results.Contains(aHateC));
                results = a.query().direction(Direction.OUT).vertices();
                Assert.AreEqual(results.Count(), 3);
                Assert.True(results.Contains(b));
                Assert.True(results.Contains(c));
                Assert.AreEqual(a.query().direction(Direction.OUT).count(), 3);


                results = a.query().direction(Direction.OUT).labels("hate", "friend").edges();
                Assert.AreEqual(results.Count(), 3);
                Assert.True(results.Contains(aFriendB));
                Assert.True(results.Contains(aFriendC));
                Assert.True(results.Contains(aHateC));
                results = a.query().direction(Direction.OUT).labels("hate", "friend").vertices();
                Assert.AreEqual(results.Count(), 3);
                Assert.True(results.Contains(b));
                Assert.True(results.Contains(c));
                Assert.AreEqual(a.query().direction(Direction.OUT).labels("hate", "friend").count(), 3);

                results = a.query().direction(Direction.OUT).labels("friend").edges();
                Assert.AreEqual(results.Count(), 2);
                Assert.True(results.Contains(aFriendB));
                Assert.True(results.Contains(aFriendC));
                results = a.query().direction(Direction.OUT).labels("friend").vertices();
                Assert.AreEqual(results.Count(), 2);
                Assert.True(results.Contains(b));
                Assert.True(results.Contains(c));
                Assert.AreEqual(a.query().direction(Direction.OUT).labels("friend").count(), 2);

                results = a.query().direction(Direction.OUT).labels("friend").has("amount", 1.0).edges();
                Assert.AreEqual(results.Count(), 1);
                Assert.True(results.Contains(aFriendB));
                results = a.query().direction(Direction.OUT).labels("friend").has("amount", 1.0).vertices();
                Assert.AreEqual(results.Count(), 1);
                Assert.True(results.Contains(b));
                Assert.AreEqual(((VertexQuery)a.query().direction(Direction.OUT).labels("friend").has("amount", 1.0)).count(), 1);

                results = a.query().direction(Direction.OUT).labels("friend").has("amount", Compare.NOT_EQUAL, 1.0).edges();
                Assert.AreEqual(results.Count(), 1);
                Assert.True(results.Contains(aFriendC));
                results = a.query().direction(Direction.OUT).labels("friend").has("amount", Compare.NOT_EQUAL, 1.0).vertices();
                Assert.AreEqual(results.Count(), 1);
                Assert.True(results.Contains(c));
                Assert.AreEqual(((VertexQuery)a.query().direction(Direction.OUT).labels("friend").has("amount", Compare.NOT_EQUAL, 1.0)).count(), 1);

                results = a.query().direction(Direction.OUT).labels("friend").has("amount", Compare.LESS_THAN_EQUAL, 1.0).edges();
                Assert.AreEqual(results.Count(), 2);
                Assert.True(results.Contains(aFriendB));
                Assert.True(results.Contains(aFriendC));
                results = a.query().direction(Direction.OUT).labels("friend").has("amount", Compare.LESS_THAN_EQUAL, 1.0).vertices();
                Assert.AreEqual(results.Count(), 2);
                Assert.True(results.Contains(b));
                Assert.True(results.Contains(c));
                Assert.AreEqual(((VertexQuery)a.query().direction(Direction.OUT).labels("friend").has("amount", Compare.LESS_THAN_EQUAL, 1.0)).count(), 2);

                results = a.query().direction(Direction.OUT).has("amount", Compare.LESS_THAN, 1.0).edges();
                Assert.AreEqual(results.Count(), 1);
                Assert.True(results.Contains(aFriendC));
                results = a.query().direction(Direction.OUT).has("amount", Compare.LESS_THAN, 1.0).vertices();
                Assert.AreEqual(results.Count(), 1);
                Assert.True(results.Contains(c));
                Assert.AreEqual(((VertexQuery)a.query().direction(Direction.OUT).has("amount", Compare.LESS_THAN, 1.0)).count(), 1);

                results = a.query().direction(Direction.OUT).labels("friend").has("amount", 0.5).edges();
                Assert.AreEqual(results.Count(), 1);
                Assert.True(results.Contains(aFriendC));
                results = a.query().direction(Direction.OUT).labels("friend").has("amount", 0.5).vertices();
                Assert.AreEqual(results.Count(), 1);
                Assert.True(results.Contains(c));

                results = a.query().direction(Direction.IN).labels("hate", "friend").has("amount", Compare.GREATER_THAN, 0.5).edges();
                Assert.AreEqual(results.Count(), 1);
                Assert.True(results.Contains(cHateA));
                results = a.query().direction(Direction.IN).labels("hate", "friend").has("amount", Compare.GREATER_THAN, 0.5).vertices();
                Assert.AreEqual(results.Count(), 1);
                Assert.True(results.Contains(c));
                Assert.AreEqual(((VertexQuery)a.query().direction(Direction.IN).labels("hate", "friend").has("amount", Compare.GREATER_THAN, 0.5)).count(), 1);

                results = a.query().direction(Direction.IN).labels("hate").has("amount", Compare.GREATER_THAN, 1.0).edges();
                Assert.AreEqual(results.Count(), 0);
                results = a.query().direction(Direction.IN).labels("hate").has("amount", Compare.GREATER_THAN, 1.0).vertices();
                Assert.AreEqual(results.Count(), 0);
                Assert.AreEqual(((VertexQuery)a.query().direction(Direction.IN).labels("hate").has("amount", Compare.GREATER_THAN, 1.0)).count(), 0);

                results = a.query().direction(Direction.IN).labels("hate").has("amount", Compare.GREATER_THAN_EQUAL, 1.0).edges();
                Assert.AreEqual(results.Count(), 1);
                Assert.True(results.Contains(cHateA));
                results = a.query().direction(Direction.IN).labels("hate").has("amount", Compare.GREATER_THAN_EQUAL, 1.0).vertices();
                Assert.AreEqual(results.Count(), 1);
                Assert.True(results.Contains(c));
                Assert.AreEqual(((VertexQuery)a.query().direction(Direction.IN).labels("hate").has("amount", Compare.GREATER_THAN_EQUAL, 1.0)).count(), 1);

                results = a.query().direction(Direction.OUT).interval("date", 5, 10).edges();
                Assert.AreEqual(results.Count(), 0);
                results = a.query().direction(Direction.OUT).interval("date", 5, 10).vertices();
                Assert.AreEqual(results.Count(), 0);
                Assert.AreEqual(((VertexQuery)a.query().direction(Direction.OUT).interval("date", 5, 10)).count(), 0);

                results = a.query().direction(Direction.OUT).interval("date", 5, 11).edges();
                Assert.AreEqual(results.Count(), 1);
                Assert.True(results.Contains(aFriendB));
                results = a.query().direction(Direction.OUT).interval("date", 5, 11).vertices();
                Assert.AreEqual(results.Count(), 1);
                Assert.True(results.Contains(b));
                Assert.AreEqual(((VertexQuery)a.query().direction(Direction.OUT).interval("date", 5, 11)).count(), 1);

                results = a.query().direction(Direction.OUT).labels("friend").interval("date", 5, 11).edges();
                Assert.AreEqual(results.Count(), 1);
                Assert.True(results.Contains(aFriendB));
                results = a.query().direction(Direction.OUT).labels("friend").interval("date", 5, 11).vertices();
                Assert.AreEqual(results.Count(), 1);
                Assert.True(results.Contains(b));
                Assert.AreEqual(((VertexQuery)a.query().direction(Direction.OUT).labels("friend").interval("date", 5, 11)).count(), 1);

                results = a.query().direction(Direction.BOTH).labels("friend", "hate").edges();
                Assert.AreEqual(results.Count(), 4);
                Assert.True(results.Contains(aFriendB));
                Assert.True(results.Contains(aFriendC));
                Assert.True(results.Contains(aHateC));
                Assert.True(results.Contains(cHateA));
                results = a.query().direction(Direction.BOTH).labels("friend", "hate").vertices();
                Assert.AreEqual(results.Count(), 4);
                Assert.True(results.Contains(b));
                Assert.True(results.Contains(c));
                Assert.False(results.Contains(a));
                Assert.AreEqual(a.query().direction(Direction.BOTH).labels("friend", "hate").count(), 4);

                results = a.query().direction(Direction.OUT).labels("friend", "hate").limit(2).edges();
                Assert.AreEqual(results.Count(), 2);
                Assert.True(results.Contains(aFriendB) || results.Contains(aHateC) || results.Contains(aFriendC));
                results = a.query().direction(Direction.OUT).labels("friend", "hate").limit(2).vertices();
                Assert.AreEqual(results.Count(), 2);
                Assert.True(results.Contains(b) || results.Contains(c));
                Assert.False(results.Contains(a));
                Assert.AreEqual(((VertexQuery)a.query().labels("friend", "hate").limit(2)).count(), 2);

                results = a.query().direction(Direction.OUT).labels("friend").limit(0).edges();
                Assert.AreEqual(results.Count(), 0);
                results = a.query().direction(Direction.OUT).labels("friend").limit(0).vertices();
                Assert.AreEqual(results.Count(), 0);
                Assert.AreEqual(((VertexQuery)a.query().direction(Direction.OUT).labels("friend").limit(0)).count(), 0);
            }
            graph.shutdown();

        }

        [Test]
        public void testGraphQueryForVertices()
        {
            Graph graph = graphTest.generateGraph();
            if (graph.getFeatures().supportsVertexIndex.Value && graph is KeyIndexableGraph) {
                ((KeyIndexableGraph) graph).createKeyIndex("name", typeof(Vertex));
            }
            if (graph.getFeatures().supportsVertexProperties.Value) {
                Vertex vertex = graph.addVertex(null);
                vertex.setProperty(convertId(graph, "name"), "marko");
                vertex.setProperty(convertId(graph, "age"), 33);
                vertex = graph.addVertex(null);
                vertex.setProperty(convertId(graph, "name"), "matthias");
                vertex.setProperty(convertId(graph, "age"), 28);
                graph.addVertex(null);

                IEnumerable<Vertex> vertices = graph.query().vertices();
                Assert.AreEqual(count(vertices), 3);
                Assert.AreEqual(count(vertices), 3);
                HashSet<string> names = new HashSet<string>();
                foreach (Vertex v in vertices)
                    names.Add((string) v.getProperty(convertId(graph, "name")));
                
                Assert.AreEqual(names.Count(), 3);
                Assert.True(names.Contains("marko"));
                Assert.True(names.Contains(null));
                Assert.True(names.Contains("matthias"));

                Assert.AreEqual(count(graph.query().limit(0).vertices()), 0);
                Assert.AreEqual(count(graph.query().limit(1).vertices()), 1);
                Assert.AreEqual(count(graph.query().limit(2).vertices()), 2);
                Assert.AreEqual(count(graph.query().limit(3).vertices()), 3);
                Assert.AreEqual(count(graph.query().limit(4).vertices()), 3);

                vertices = graph.query().has("name", "marko").vertices();
                Assert.AreEqual(count(vertices), 1);
                // Assert.AreEqual(vertices.First().GetProperty("name"), "marko");

                vertices = graph.query().has("age", Compare.GREATER_THAN_EQUAL, 29).vertices();
                Assert.AreEqual(count(vertices), 1);
                Assert.AreEqual(vertices.First().getProperty("name"), "marko");
                Assert.AreEqual(vertices.First().getProperty("age"), 33);

                vertices = graph.query().has("age", Compare.GREATER_THAN_EQUAL, 28).vertices();
                Assert.AreEqual(count(vertices), 2);
                names = new HashSet<string>();
                foreach (Vertex v in vertices)
                    names.Add((string) v.getProperty(convertId(graph, "name")));
                
                Assert.AreEqual(names.Count(), 2);
                Assert.True(names.Contains("marko"));
                Assert.True(names.Contains("matthias"));

                vertices = graph.query().interval("age", 28, 33).vertices();
                Assert.AreEqual(count(vertices), 1);
                Assert.AreEqual(vertices.First().getProperty("name"), "matthias");

                Assert.AreEqual(count(graph.query().has("age", null).vertices()), 1);
                Assert.AreEqual(count(graph.query().has("age", 28).has("name", "matthias").vertices()), 1);
                Assert.AreEqual(count(graph.query().has("age", 28).has("name", "matthias").has("name", "matthias").vertices()), 1);
                Assert.AreEqual(count(graph.query().interval("age", 28, 32).has("name", "marko").vertices()), 0);
                graph.shutdown();
            }
        }

        [Test]
        public void testGraphQueryForEdges()
        {
            Graph graph = graphTest.generateGraph();
            if (graph.getFeatures().supportsEdgeIndex.Value && graph is KeyIndexableGraph)
                ((KeyIndexableGraph) graph).createKeyIndex("type", typeof(Edge));
            
            if (graph.getFeatures().supportsEdgeProperties.Value && graph.getFeatures().supportsVertexProperties.Value)
            {
                Vertex marko = graph.addVertex(null);
                marko.setProperty("name", "marko");
                Vertex matthias = graph.addVertex(null);
                matthias.setProperty("name", "matthias");
                Vertex stephen = graph.addVertex(null);
                stephen.setProperty("name", "stephen");

                Edge edge = marko.addEdge("knows", stephen);
                edge.setProperty("type", "tinkerpop");
                edge.setProperty("weight", 1.0);
                edge = marko.addEdge("knows", matthias);
                edge.setProperty("type", "aurelius");

                Assert.AreEqual(count(graph.query().edges()), 2);
                Assert.AreEqual(count(graph.query().limit(0).edges()), 0);
                Assert.AreEqual(count(graph.query().limit(1).edges()), 1);
                Assert.AreEqual(count(graph.query().limit(2).edges()), 2);
                Assert.AreEqual(count(graph.query().limit(3).edges()), 2);

                Assert.AreEqual(count(graph.query().has("type", "tinkerpop").has("type", "tinkerpop").edges()), 1);
                Assert.AreEqual(graph.query().has("type", "tinkerpop").edges().First().getProperty("weight"), 1.0);
                Assert.AreEqual(count(graph.query().has("type", "aurelius").edges()), 1);
                Assert.AreEqual(graph.query().has("type", "aurelius").edges().First().getPropertyKeys().Count(), 1);
                Assert.AreEqual(count(graph.query().has("weight", null).edges()), 1);
                Assert.AreEqual(graph.query().has("weight", null).edges().First().getProperty("type"), "aurelius");

                Assert.AreEqual(count(graph.query().has("weight", 1.0).edges()), 1);
                Assert.AreEqual(graph.query().has("weight", 1.0).edges().First().getProperty("type"), "tinkerpop");
                Assert.AreEqual(count(graph.query().has("weight", 1.0).has("type", "tinkerpop").edges()), 1);
                Assert.AreEqual(graph.query().has("weight", 1.0).has("type", "tinkerpop").edges().First().getProperty("type"), "tinkerpop");
                Assert.AreEqual(count(((Query) graph.query().has("weight", 1.0)).has("type", "aurelius").edges()), 0);

                Assert.AreEqual(graph.query().interval("weight", 0.0, 1.1).edges().First().getProperty("type"), "tinkerpop");
                Assert.AreEqual(count(graph.query().interval("weight", 0.0, 1.0).edges()), 0);
            }
            graph.shutdown();
        }
    }
}
