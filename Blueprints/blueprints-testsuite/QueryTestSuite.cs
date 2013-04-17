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
        public void TestVertexQuery()
        {
            Graph graph = _GraphTest.GenerateGraph();
            if (graph.GetFeatures().SupportsEdgeProperties.Value)
            {

                Vertex a = graph.AddVertex(null);
                Vertex b = graph.AddVertex(null);
                Vertex c = graph.AddVertex(null);
                Edge aFriendB = graph.AddEdge(null, a, b, ConvertId(graph, "friend"));
                Edge aFriendC = graph.AddEdge(null, a, c, ConvertId(graph, "friend"));
                Edge aHateC = graph.AddEdge(null, a, c, ConvertId(graph, "hate"));
                Edge cHateA = graph.AddEdge(null, c, a, ConvertId(graph, "hate"));
                Edge cHateB = graph.AddEdge(null, c, b, ConvertId(graph, "hate"));
                aFriendB.SetProperty("amount", 1.0);
                aFriendB.SetProperty("date", 10);
                aFriendC.SetProperty("amount", 0.5);
                aHateC.SetProperty("amount", 1.0);
                cHateA.SetProperty("amount", 1.0);
                cHateB.SetProperty("amount", 0.4);

                Assert.AreEqual(Count(a.Query().Labels("friend").Has("date", null).Edges()), 1);
                Assert.AreEqual(a.Query().Labels("friend").Has("date", null).Edges().First().GetProperty("amount"), 0.5);

                // out Edges

                IEnumerable<object> results = a.Query().Direction(Direction.OUT).Edges();
                Assert.AreEqual(results.Count(), 3);
                Assert.True(results.Contains(aFriendB));
                Assert.True(results.Contains(aFriendC));
                Assert.True(results.Contains(aHateC));
                results = a.Query().Direction(Direction.OUT).Vertices();
                Assert.AreEqual(results.Count(), 3);
                Assert.True(results.Contains(b));
                Assert.True(results.Contains(c));
                Assert.AreEqual(a.Query().Direction(Direction.OUT).Count(), 3);


                results = a.Query().Direction(Direction.OUT).Labels("hate", "friend").Edges();
                Assert.AreEqual(results.Count(), 3);
                Assert.True(results.Contains(aFriendB));
                Assert.True(results.Contains(aFriendC));
                Assert.True(results.Contains(aHateC));
                results = a.Query().Direction(Direction.OUT).Labels("hate", "friend").Vertices();
                Assert.AreEqual(results.Count(), 3);
                Assert.True(results.Contains(b));
                Assert.True(results.Contains(c));
                Assert.AreEqual(a.Query().Direction(Direction.OUT).Labels("hate", "friend").Count(), 3);

                results = a.Query().Direction(Direction.OUT).Labels("friend").Edges();
                Assert.AreEqual(results.Count(), 2);
                Assert.True(results.Contains(aFriendB));
                Assert.True(results.Contains(aFriendC));
                results = a.Query().Direction(Direction.OUT).Labels("friend").Vertices();
                Assert.AreEqual(results.Count(), 2);
                Assert.True(results.Contains(b));
                Assert.True(results.Contains(c));
                Assert.AreEqual(a.Query().Direction(Direction.OUT).Labels("friend").Count(), 2);

                results = a.Query().Direction(Direction.OUT).Labels("friend").Has("amount", 1.0).Edges();
                Assert.AreEqual(results.Count(), 1);
                Assert.True(results.Contains(aFriendB));
                results = a.Query().Direction(Direction.OUT).Labels("friend").Has("amount", 1.0).Vertices();
                Assert.AreEqual(results.Count(), 1);
                Assert.True(results.Contains(b));
                Assert.AreEqual(((VertexQuery)a.Query().Direction(Direction.OUT).Labels("friend").Has("amount", 1.0)).Count(), 1);

                results = a.Query().Direction(Direction.OUT).Labels("friend").Has("amount", Compare.NOT_EQUAL, 1.0).Edges();
                Assert.AreEqual(results.Count(), 1);
                Assert.True(results.Contains(aFriendC));
                results = a.Query().Direction(Direction.OUT).Labels("friend").Has("amount", Compare.NOT_EQUAL, 1.0).Vertices();
                Assert.AreEqual(results.Count(), 1);
                Assert.True(results.Contains(c));
                Assert.AreEqual(((VertexQuery)a.Query().Direction(Direction.OUT).Labels("friend").Has("amount", Compare.NOT_EQUAL, 1.0)).Count(), 1);

                results = a.Query().Direction(Direction.OUT).Labels("friend").Has("amount", Compare.LESS_THAN_EQUAL, 1.0).Edges();
                Assert.AreEqual(results.Count(), 2);
                Assert.True(results.Contains(aFriendB));
                Assert.True(results.Contains(aFriendC));
                results = a.Query().Direction(Direction.OUT).Labels("friend").Has("amount", Compare.LESS_THAN_EQUAL, 1.0).Vertices();
                Assert.AreEqual(results.Count(), 2);
                Assert.True(results.Contains(b));
                Assert.True(results.Contains(c));
                Assert.AreEqual(((VertexQuery)a.Query().Direction(Direction.OUT).Labels("friend").Has("amount", Compare.LESS_THAN_EQUAL, 1.0)).Count(), 2);

                results = a.Query().Direction(Direction.OUT).Has("amount", Compare.LESS_THAN, 1.0).Edges();
                Assert.AreEqual(results.Count(), 1);
                Assert.True(results.Contains(aFriendC));
                results = a.Query().Direction(Direction.OUT).Has("amount", Compare.LESS_THAN, 1.0).Vertices();
                Assert.AreEqual(results.Count(), 1);
                Assert.True(results.Contains(c));
                Assert.AreEqual(((VertexQuery)a.Query().Direction(Direction.OUT).Has("amount", Compare.LESS_THAN, 1.0)).Count(), 1);

                results = a.Query().Direction(Direction.OUT).Labels("friend").Has("amount", 0.5).Edges();
                Assert.AreEqual(results.Count(), 1);
                Assert.True(results.Contains(aFriendC));
                results = a.Query().Direction(Direction.OUT).Labels("friend").Has("amount", 0.5).Vertices();
                Assert.AreEqual(results.Count(), 1);
                Assert.True(results.Contains(c));

                results = a.Query().Direction(Direction.IN).Labels("hate", "friend").Has("amount", Compare.GREATER_THAN, 0.5).Edges();
                Assert.AreEqual(results.Count(), 1);
                Assert.True(results.Contains(cHateA));
                results = a.Query().Direction(Direction.IN).Labels("hate", "friend").Has("amount", Compare.GREATER_THAN, 0.5).Vertices();
                Assert.AreEqual(results.Count(), 1);
                Assert.True(results.Contains(c));
                Assert.AreEqual(((VertexQuery)a.Query().Direction(Direction.IN).Labels("hate", "friend").Has("amount", Compare.GREATER_THAN, 0.5)).Count(), 1);

                results = a.Query().Direction(Direction.IN).Labels("hate").Has("amount", Compare.GREATER_THAN, 1.0).Edges();
                Assert.AreEqual(results.Count(), 0);
                results = a.Query().Direction(Direction.IN).Labels("hate").Has("amount", Compare.GREATER_THAN, 1.0).Vertices();
                Assert.AreEqual(results.Count(), 0);
                Assert.AreEqual(((VertexQuery)a.Query().Direction(Direction.IN).Labels("hate").Has("amount", Compare.GREATER_THAN, 1.0)).Count(), 0);

                results = a.Query().Direction(Direction.IN).Labels("hate").Has("amount", Compare.GREATER_THAN_EQUAL, 1.0).Edges();
                Assert.AreEqual(results.Count(), 1);
                Assert.True(results.Contains(cHateA));
                results = a.Query().Direction(Direction.IN).Labels("hate").Has("amount", Compare.GREATER_THAN_EQUAL, 1.0).Vertices();
                Assert.AreEqual(results.Count(), 1);
                Assert.True(results.Contains(c));
                Assert.AreEqual(((VertexQuery)a.Query().Direction(Direction.IN).Labels("hate").Has("amount", Compare.GREATER_THAN_EQUAL, 1.0)).Count(), 1);

                results = a.Query().Direction(Direction.OUT).Interval("date", 5, 10).Edges();
                Assert.AreEqual(results.Count(), 0);
                results = a.Query().Direction(Direction.OUT).Interval("date", 5, 10).Vertices();
                Assert.AreEqual(results.Count(), 0);
                Assert.AreEqual(((VertexQuery)a.Query().Direction(Direction.OUT).Interval("date", 5, 10)).Count(), 0);

                results = a.Query().Direction(Direction.OUT).Interval("date", 5, 11).Edges();
                Assert.AreEqual(results.Count(), 1);
                Assert.True(results.Contains(aFriendB));
                results = a.Query().Direction(Direction.OUT).Interval("date", 5, 11).Vertices();
                Assert.AreEqual(results.Count(), 1);
                Assert.True(results.Contains(b));
                Assert.AreEqual(((VertexQuery)a.Query().Direction(Direction.OUT).Interval("date", 5, 11)).Count(), 1);

                results = a.Query().Direction(Direction.OUT).Labels("friend").Interval("date", 5, 11).Edges();
                Assert.AreEqual(results.Count(), 1);
                Assert.True(results.Contains(aFriendB));
                results = a.Query().Direction(Direction.OUT).Labels("friend").Interval("date", 5, 11).Vertices();
                Assert.AreEqual(results.Count(), 1);
                Assert.True(results.Contains(b));
                Assert.AreEqual(((VertexQuery)a.Query().Direction(Direction.OUT).Labels("friend").Interval("date", 5, 11)).Count(), 1);

                results = a.Query().Direction(Direction.BOTH).Labels("friend", "hate").Edges();
                Assert.AreEqual(results.Count(), 4);
                Assert.True(results.Contains(aFriendB));
                Assert.True(results.Contains(aFriendC));
                Assert.True(results.Contains(aHateC));
                Assert.True(results.Contains(cHateA));
                results = a.Query().Direction(Direction.BOTH).Labels("friend", "hate").Vertices();
                Assert.AreEqual(results.Count(), 4);
                Assert.True(results.Contains(b));
                Assert.True(results.Contains(c));
                Assert.False(results.Contains(a));
                Assert.AreEqual(a.Query().Direction(Direction.BOTH).Labels("friend", "hate").Count(), 4);

                results = a.Query().Direction(Direction.OUT).Labels("friend", "hate").Limit(2).Edges();
                Assert.AreEqual(results.Count(), 2);
                Assert.True(results.Contains(aFriendB) || results.Contains(aHateC) || results.Contains(aFriendC));
                results = a.Query().Direction(Direction.OUT).Labels("friend", "hate").Limit(2).Vertices();
                Assert.AreEqual(results.Count(), 2);
                Assert.True(results.Contains(b) || results.Contains(c));
                Assert.False(results.Contains(a));
                Assert.AreEqual(((VertexQuery)a.Query().Labels("friend", "hate").Limit(2)).Count(), 2);

                results = a.Query().Direction(Direction.OUT).Labels("friend").Limit(0).Edges();
                Assert.AreEqual(results.Count(), 0);
                results = a.Query().Direction(Direction.OUT).Labels("friend").Limit(0).Vertices();
                Assert.AreEqual(results.Count(), 0);
                Assert.AreEqual(((VertexQuery)a.Query().Direction(Direction.OUT).Labels("friend").Limit(0)).Count(), 0);
            }
            graph.Shutdown();

        }

        [Test]
        public void TestGraphQueryForVertices()
        {
            Graph graph = _GraphTest.GenerateGraph();
            if (graph.GetFeatures().SupportsVertexIndex.Value && graph is KeyIndexableGraph) {
                ((KeyIndexableGraph) graph).CreateKeyIndex("name", typeof(Vertex));
            }
            if (graph.GetFeatures().SupportsVertexProperties.Value) {
                Vertex vertex = graph.AddVertex(null);
                vertex.SetProperty(ConvertId(graph, "name"), "marko");
                vertex.SetProperty(ConvertId(graph, "age"), 33);
                vertex = graph.AddVertex(null);
                vertex.SetProperty(ConvertId(graph, "name"), "matthias");
                vertex.SetProperty(ConvertId(graph, "age"), 28);
                graph.AddVertex(null);

                IEnumerable<Vertex> vertices = graph.Query().Vertices();
                Assert.AreEqual(Count(vertices), 3);
                Assert.AreEqual(Count(vertices), 3);
                HashSet<string> names = new HashSet<string>();
                foreach (Vertex v in vertices)
                    names.Add((string) v.GetProperty(ConvertId(graph, "name")));
                
                Assert.AreEqual(names.Count(), 3);
                Assert.True(names.Contains("marko"));
                Assert.True(names.Contains(null));
                Assert.True(names.Contains("matthias"));

                Assert.AreEqual(Count(graph.Query().Limit(0).Vertices()), 0);
                Assert.AreEqual(Count(graph.Query().Limit(1).Vertices()), 1);
                Assert.AreEqual(Count(graph.Query().Limit(2).Vertices()), 2);
                Assert.AreEqual(Count(graph.Query().Limit(3).Vertices()), 3);
                Assert.AreEqual(Count(graph.Query().Limit(4).Vertices()), 3);

                vertices = graph.Query().Has("name", "marko").Vertices();
                Assert.AreEqual(Count(vertices), 1);
                // Assert.AreEqual(vertices.First().GetProperty("name"), "marko");

                vertices = graph.Query().Has("age", Compare.GREATER_THAN_EQUAL, 29).Vertices();
                Assert.AreEqual(Count(vertices), 1);
                Assert.AreEqual(vertices.First().GetProperty("name"), "marko");
                Assert.AreEqual(vertices.First().GetProperty("age"), 33);

                vertices = graph.Query().Has("age", Compare.GREATER_THAN_EQUAL, 28).Vertices();
                Assert.AreEqual(Count(vertices), 2);
                names = new HashSet<string>();
                foreach (Vertex v in vertices)
                    names.Add((string) v.GetProperty(ConvertId(graph, "name")));
                
                Assert.AreEqual(names.Count(), 2);
                Assert.True(names.Contains("marko"));
                Assert.True(names.Contains("matthias"));

                vertices = graph.Query().Interval("age", 28, 33).Vertices();
                Assert.AreEqual(Count(vertices), 1);
                Assert.AreEqual(vertices.First().GetProperty("name"), "matthias");

                Assert.AreEqual(Count(graph.Query().Has("age", null).Vertices()), 1);
                Assert.AreEqual(Count(graph.Query().Has("age", 28).Has("name", "matthias").Vertices()), 1);
                Assert.AreEqual(Count(graph.Query().Has("age", 28).Has("name", "matthias").Has("name", "matthias").Vertices()), 1);
                Assert.AreEqual(Count(graph.Query().Interval("age", 28, 32).Has("name", "marko").Vertices()), 0);
                graph.Shutdown();
            }
        }

        [Test]
        public void TestGraphQueryForEdges()
        {
            Graph graph = _GraphTest.GenerateGraph();
            if (graph.GetFeatures().SupportsEdgeIndex.Value && graph is KeyIndexableGraph)
                ((KeyIndexableGraph) graph).CreateKeyIndex("type", typeof(Edge));
            
            if (graph.GetFeatures().SupportsEdgeProperties.Value && graph.GetFeatures().SupportsVertexProperties.Value)
            {
                Vertex marko = graph.AddVertex(null);
                marko.SetProperty("name", "marko");
                Vertex matthias = graph.AddVertex(null);
                matthias.SetProperty("name", "matthias");
                Vertex stephen = graph.AddVertex(null);
                stephen.SetProperty("name", "stephen");

                Edge edge = marko.AddEdge("knows", stephen);
                edge.SetProperty("type", "tinkerpop");
                edge.SetProperty("weight", 1.0);
                edge = marko.AddEdge("knows", matthias);
                edge.SetProperty("type", "aurelius");

                Assert.AreEqual(Count(graph.Query().Edges()), 2);
                Assert.AreEqual(Count(graph.Query().Limit(0).Edges()), 0);
                Assert.AreEqual(Count(graph.Query().Limit(1).Edges()), 1);
                Assert.AreEqual(Count(graph.Query().Limit(2).Edges()), 2);
                Assert.AreEqual(Count(graph.Query().Limit(3).Edges()), 2);

                Assert.AreEqual(Count(graph.Query().Has("type", "tinkerpop").Has("type", "tinkerpop").Edges()), 1);
                Assert.AreEqual(graph.Query().Has("type", "tinkerpop").Edges().First().GetProperty("weight"), 1.0);
                Assert.AreEqual(Count(graph.Query().Has("type", "aurelius").Edges()), 1);
                Assert.AreEqual(graph.Query().Has("type", "aurelius").Edges().First().GetPropertyKeys().Count(), 1);
                Assert.AreEqual(Count(graph.Query().Has("weight", null).Edges()), 1);
                Assert.AreEqual(graph.Query().Has("weight", null).Edges().First().GetProperty("type"), "aurelius");

                Assert.AreEqual(Count(graph.Query().Has("weight", 1.0).Edges()), 1);
                Assert.AreEqual(graph.Query().Has("weight", 1.0).Edges().First().GetProperty("type"), "tinkerpop");
                Assert.AreEqual(Count(graph.Query().Has("weight", 1.0).Has("type", "tinkerpop").Edges()), 1);
                Assert.AreEqual(graph.Query().Has("weight", 1.0).Has("type", "tinkerpop").Edges().First().GetProperty("type"), "tinkerpop");
                Assert.AreEqual(Count(graph.Query().Has("weight", 1.0).Has("type", "aurelius").Edges()), 0);

                Assert.AreEqual(graph.Query().Interval("weight", 0.0, 1.1).Edges().First().GetProperty("type"), "tinkerpop");
                Assert.AreEqual(Count(graph.Query().Interval("weight", 0.0, 1.0).Edges()), 0);
            }
            graph.Shutdown();
        }
    }
}
