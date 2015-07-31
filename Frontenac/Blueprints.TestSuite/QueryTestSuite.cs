using System.Collections.Generic;
using System.Linq;
using Frontenac.Blueprints.Impls;
using NUnit.Framework;

namespace Frontenac.Blueprints
{
    public abstract class QueryTestSuite : TestSuite
    {
        protected QueryTestSuite(GraphTest graphTest)
            : base("QueryTestSuite", graphTest)
        {
        }

        [Test]
        public void TestVertexQuery()
        {
            var graph = GraphTest.GenerateGraph();
            try
            {
                if (!graph.Features.SupportsEdgeProperties) return;

                var a = graph.AddVertex(null);
                var b = graph.AddVertex(null);
                var c = graph.AddVertex(null);
                var aFriendB = graph.AddEdge(null, a, b, ConvertId(graph, "friend"));
                var aFriendC = graph.AddEdge(null, a, c, ConvertId(graph, "friend"));
                var aHateC = graph.AddEdge(null, a, c, ConvertId(graph, "hate"));
                var cHateA = graph.AddEdge(null, c, a, ConvertId(graph, "hate"));
                var cHateB = graph.AddEdge(null, c, b, ConvertId(graph, "hate"));
                aFriendB.SetProperty("amount", 1.0);
                aFriendB.SetProperty("date", 10);
                aFriendC.SetProperty("amount", 0.5);
                aHateC.SetProperty("amount", 1.0);
                cHateA.SetProperty("amount", 1.0);
                cHateB.SetProperty("amount", 0.4);

                Assert.AreEqual(Count(a.Query().Labels("friend").Has("date", null).Edges()), 1);
                Assert.AreEqual(a.Query().Labels("friend").Has("date", null).Edges().First().GetProperty("amount"), 0.5);

                // out Edges

                IEnumerable<object> results = a.Query().Direction(Direction.Out).Edges().ToArray();
                Assert.AreEqual(results.Count(), 3);
                Assert.True(results.Contains(aFriendB));
                Assert.True(results.Contains(aFriendC));
                Assert.True(results.Contains(aHateC));
                results = a.Query().Direction(Direction.Out).Vertices().ToArray();
                Assert.AreEqual(results.Count(), 3);
                Assert.True(results.Contains(b));
                Assert.True(results.Contains(c));
                Assert.AreEqual(a.Query().Direction(Direction.Out).Count(), 3);

                results = a.Query().Direction(Direction.Out).Labels("hate", "friend").Edges().ToArray();
                Assert.AreEqual(results.Count(), 3);
                Assert.True(results.Contains(aFriendB));
                Assert.True(results.Contains(aFriendC));
                Assert.True(results.Contains(aHateC));
                results = a.Query().Direction(Direction.Out).Labels("hate", "friend").Vertices().ToArray();
                Assert.AreEqual(results.Count(), 3);
                Assert.True(results.Contains(b));
                Assert.True(results.Contains(c));
                Assert.AreEqual(a.Query().Direction(Direction.Out).Labels("hate", "friend").Count(), 3);

                results = a.Query().Direction(Direction.Out).Labels("friend").Edges().ToArray();
                Assert.AreEqual(results.Count(), 2);
                Assert.True(results.Contains(aFriendB));
                Assert.True(results.Contains(aFriendC));
                results = a.Query().Direction(Direction.Out).Labels("friend").Vertices().ToArray();
                Assert.AreEqual(results.Count(), 2);
                Assert.True(results.Contains(b));
                Assert.True(results.Contains(c));
                Assert.AreEqual(a.Query().Direction(Direction.Out).Labels("friend").Count(), 2);

                results = a.Query().Direction(Direction.Out).Labels("friend").Has("amount", 1.0).Edges().ToArray();
                Assert.AreEqual(results.Count(), 1);
                Assert.True(results.Contains(aFriendB));
                results = a.Query().Direction(Direction.Out).Labels("friend").Has("amount", 1.0).Vertices().ToArray();
                Assert.AreEqual(results.Count(), 1);
                Assert.True(results.Contains(b));
                Assert.AreEqual(
                    ((IVertexQuery) a.Query().Direction(Direction.Out).Labels("friend").Has("amount", 1.0)).Count(), 1);

                results =
                    a.Query()
                     .Direction(Direction.Out)
                     .Labels("friend")
                     .Has("amount", Compare.NotEqual, 1.0)
                     .Edges()
                     .ToArray();
                Assert.AreEqual(results.Count(), 1);
                Assert.True(results.Contains(aFriendC));
                results =
                    a.Query()
                     .Direction(Direction.Out)
                     .Labels("friend")
                     .Has("amount", Compare.NotEqual, 1.0)
                     .Vertices()
                     .ToArray();
                Assert.AreEqual(results.Count(), 1);
                Assert.True(results.Contains(c));
                Assert.AreEqual(
                    ((IVertexQuery)
                     a.Query().Direction(Direction.Out).Labels("friend").Has("amount", Compare.NotEqual, 1.0)).Count(),
                    1);

                results =
                    a.Query()
                     .Direction(Direction.Out)
                     .Labels("friend")
                     .Has("amount", Compare.LessThanEqual, 1.0)
                     .Edges()
                     .ToArray();
                Assert.AreEqual(results.Count(), 2);
                Assert.True(results.Contains(aFriendB));
                Assert.True(results.Contains(aFriendC));
                results =
                    a.Query()
                     .Direction(Direction.Out)
                     .Labels("friend")
                     .Has("amount", Compare.LessThanEqual, 1.0)
                     .Vertices()
                     .ToArray();
                Assert.AreEqual(results.Count(), 2);
                Assert.True(results.Contains(b));
                Assert.True(results.Contains(c));
                Assert.AreEqual(
                    ((IVertexQuery)
                     a.Query().Direction(Direction.Out).Labels("friend").Has("amount", Compare.LessThanEqual, 1.0))
                        .Count(), 2);

                results = a.Query().Direction(Direction.Out).Has("amount", Compare.LessThan, 1.0).Edges().ToArray();
                Assert.AreEqual(results.Count(), 1);
                Assert.True(results.Contains(aFriendC));
                results = a.Query().Direction(Direction.Out).Has("amount", Compare.LessThan, 1.0).Vertices().ToArray();
                Assert.AreEqual(results.Count(), 1);
                Assert.True(results.Contains(c));
                Assert.AreEqual(
                    ((IVertexQuery) a.Query().Direction(Direction.Out).Has("amount", Compare.LessThan, 1.0)).Count(), 1);

                results = a.Query().Direction(Direction.Out).Labels("friend").Has("amount", 0.5).Edges().ToArray();
                Assert.AreEqual(results.Count(), 1);
                Assert.True(results.Contains(aFriendC));
                results = a.Query().Direction(Direction.Out).Labels("friend").Has("amount", 0.5).Vertices().ToArray();
                Assert.AreEqual(results.Count(), 1);
                Assert.True(results.Contains(c));

                results =
                    a.Query()
                     .Direction(Direction.In)
                     .Labels("hate", "friend")
                     .Has("amount", Compare.GreaterThan, 0.5)
                     .Edges()
                     .ToArray();
                Assert.AreEqual(results.Count(), 1);
                Assert.True(results.Contains(cHateA));
                results =
                    a.Query()
                     .Direction(Direction.In)
                     .Labels("hate", "friend")
                     .Has("amount", Compare.GreaterThan, 0.5)
                     .Vertices();
                Assert.AreEqual(results.Count(), 1);
                Assert.True(results.Contains(c));
                Assert.AreEqual(
                    ((IVertexQuery)
                     a.Query().Direction(Direction.In).Labels("hate", "friend").Has("amount", Compare.GreaterThan, 0.5))
                        .Count(), 1);

                results =
                    a.Query().Direction(Direction.In).Labels("hate").Has("amount", Compare.GreaterThan, 1.0).Edges();
                Assert.AreEqual(results.Count(), 0);
                results =
                    a.Query()
                     .Direction(Direction.In)
                     .Labels("hate")
                     .Has("amount", Compare.GreaterThan, 1.0)
                     .Vertices()
                     .ToArray();
                Assert.AreEqual(results.Count(), 0);
                Assert.AreEqual(
                    ((IVertexQuery)
                     a.Query().Direction(Direction.In).Labels("hate").Has("amount", Compare.GreaterThan, 1.0)).Count(),
                    0);

                results =
                    a.Query()
                     .Direction(Direction.In)
                     .Labels("hate")
                     .Has("amount", Compare.GreaterThanEqual, 1.0)
                     .Edges()
                     .ToArray();
                Assert.AreEqual(results.Count(), 1);
                Assert.True(results.Contains(cHateA));
                results =
                    a.Query()
                     .Direction(Direction.In)
                     .Labels("hate")
                     .Has("amount", Compare.GreaterThanEqual, 1.0)
                     .Vertices();
                Assert.AreEqual(results.Count(), 1);
                Assert.True(results.Contains(c));
                Assert.AreEqual(
                    ((IVertexQuery)
                     a.Query().Direction(Direction.In).Labels("hate").Has("amount", Compare.GreaterThanEqual, 1.0))
                        .Count(), 1);

                results = a.Query().Direction(Direction.Out).Interval("date", 5, 10).Edges();
                Assert.AreEqual(results.Count(), 0);
                results = a.Query().Direction(Direction.Out).Interval("date", 5, 10).Vertices().ToArray();
                Assert.AreEqual(results.Count(), 0);
                Assert.AreEqual(((IVertexQuery) a.Query().Direction(Direction.Out).Interval("date", 5, 10)).Count(), 0);

                results = a.Query().Direction(Direction.Out).Interval("date", 5, 11).Edges().ToArray();
                Assert.AreEqual(results.Count(), 1);
                Assert.True(results.Contains(aFriendB));
                results = a.Query().Direction(Direction.Out).Interval("date", 5, 11).Vertices().ToArray();
                Assert.AreEqual(results.Count(), 1);
                Assert.True(results.Contains(b));
                Assert.AreEqual(((IVertexQuery) a.Query().Direction(Direction.Out).Interval("date", 5, 11)).Count(), 1);

                results = a.Query().Direction(Direction.Out).Labels("friend").Interval("date", 5, 11).Edges().ToArray();
                Assert.AreEqual(results.Count(), 1);
                Assert.True(results.Contains(aFriendB));
                results =
                    a.Query().Direction(Direction.Out).Labels("friend").Interval("date", 5, 11).Vertices().ToArray();
                Assert.AreEqual(results.Count(), 1);
                Assert.True(results.Contains(b));
                Assert.AreEqual(
                    ((IVertexQuery) a.Query().Direction(Direction.Out).Labels("friend").Interval("date", 5, 11)).Count(),
                    1);

                results = a.Query().Direction(Direction.Both).Labels("friend", "hate").Edges().ToArray();
                Assert.AreEqual(results.Count(), 4);
                Assert.True(results.Contains(aFriendB));
                Assert.True(results.Contains(aFriendC));
                Assert.True(results.Contains(aHateC));
                Assert.True(results.Contains(cHateA));
                results = a.Query().Direction(Direction.Both).Labels("friend", "hate").Vertices().ToArray();
                Assert.AreEqual(results.Count(), 4);
                Assert.True(results.Contains(b));
                Assert.True(results.Contains(c));
                Assert.False(results.Contains(a));
                Assert.AreEqual(a.Query().Direction(Direction.Both).Labels("friend", "hate").Count(), 4);

                results = a.Query().Direction(Direction.Out).Labels("friend", "hate").Limit(2).Edges().ToArray();
                Assert.AreEqual(results.Count(), 2);
                Assert.True(results.Contains(aFriendB) || results.Contains(aHateC) || results.Contains(aFriendC));
                results = a.Query().Direction(Direction.Out).Labels("friend", "hate").Limit(2).Vertices();
                Assert.AreEqual(results.Count(), 2);
                Assert.True(results.Contains(b) || results.Contains(c));
                Assert.False(results.Contains(a));
                Assert.AreEqual(((IVertexQuery) a.Query().Labels("friend", "hate").Limit(2)).Count(), 2);

                results = a.Query().Direction(Direction.Out).Labels("friend").Limit(0).Edges();
                Assert.AreEqual(results.Count(), 0);
                results = a.Query().Direction(Direction.Out).Labels("friend").Limit(0).Vertices();
                Assert.AreEqual(results.Count(), 0);
                Assert.AreEqual(((IVertexQuery) a.Query().Direction(Direction.Out).Labels("friend").Limit(0)).Count(), 0);
            }
            finally
            {
                graph.Shutdown();
            }
        }

        [Test]
        public void TestGraphQueryForVertices()
        {
            var graph = GraphTest.GenerateGraph();
            try
            {
                if (graph.Features.SupportsVertexIndex && graph is IKeyIndexableGraph)
                {
                    ((IKeyIndexableGraph) graph).CreateKeyIndex("name", typeof (IVertex));
                }

                if (!graph.Features.SupportsVertexProperties) return;

                var vertex = graph.AddVertex(null);
                vertex.SetProperty(ConvertId(graph, "name"), "marko");
                vertex.SetProperty(ConvertId(graph, "age"), 33);
                vertex = graph.AddVertex(null);
                vertex.SetProperty(ConvertId(graph, "name"), "matthias");
                vertex.SetProperty(ConvertId(graph, "age"), 28);
                graph.AddVertex(null);

                var vertices = graph.Query().Vertices().ToArray();
                Assert.AreEqual(Count(vertices), 3);
                Assert.AreEqual(Count(vertices), 3);
                var names = new HashSet<string>();
                foreach (var v in vertices)
                    names.Add((string) v.GetProperty(ConvertId(graph, "name")));

                Assert.AreEqual(names.Count(), 3);
                Assert.True(names.Contains("marko"));
                // ReSharper disable AssignNullToNotNullAttribute
                Assert.True(names.Contains(null));
                // ReSharper restore AssignNullToNotNullAttribute
                Assert.True(names.Contains("matthias"));

                Assert.AreEqual(Count(graph.Query().Limit(0).Vertices()), 0);
                Assert.AreEqual(Count(graph.Query().Limit(1).Vertices()), 1);
                Assert.AreEqual(Count(graph.Query().Limit(2).Vertices()), 2);
                Assert.AreEqual(Count(graph.Query().Limit(3).Vertices()), 3);
                Assert.AreEqual(Count(graph.Query().Limit(4).Vertices()), 3);

                vertices = graph.Query().Has("name", "marko").Vertices().ToArray();
                Assert.AreEqual(Count(vertices), 1);

                vertices = graph.Query().Has("age", Compare.GreaterThanEqual, 29).Vertices().ToArray();
                Assert.AreEqual(Count(vertices), 1);
                Assert.AreEqual(vertices.First().GetProperty("name"), "marko");
                Assert.AreEqual(vertices.First().GetProperty("age"), 33);

                vertices = graph.Query().Has("age", Compare.GreaterThanEqual, 28).Vertices().ToArray();
                Assert.AreEqual(Count(vertices), 2);
                names = new HashSet<string>();
                foreach (var v in vertices)
                    names.Add((string) v.GetProperty(ConvertId(graph, "name")));

                Assert.AreEqual(names.Count(), 2);
                Assert.True(names.Contains("marko"));
                Assert.True(names.Contains("matthias"));

                vertices = graph.Query().Interval("age", 28, 33).Vertices().ToArray();
                Assert.AreEqual(Count(vertices), 1);
                Assert.AreEqual(vertices.First().GetProperty("name"), "matthias");

                Assert.AreEqual(Count(graph.Query().Has("age", null).Vertices()), 1);
                Assert.AreEqual(Count(graph.Query().Has("age", 28).Has("name", "matthias").Vertices()), 1);
                Assert.AreEqual(
                    Count(graph.Query().Has("age", 28).Has("name", "matthias").Has("name", "matthias").Vertices()), 1);
                Assert.AreEqual(Count(graph.Query().Interval("age", 28, 32).Has("name", "marko").Vertices()), 0);
            }
            finally
            {
                graph.Shutdown();
            }
        }

        [Test]
        public void TestGraphQueryForEdges()
        {
            var graph = GraphTest.GenerateGraph();
            try
            {
                if (graph.Features.SupportsEdgeIndex && graph is IKeyIndexableGraph)
                    ((IKeyIndexableGraph) graph).CreateKeyIndex("type", typeof (IEdge));

                if (!graph.Features.SupportsEdgeProperties || !graph.Features.SupportsVertexProperties) return;

                var marko = graph.AddVertex(null);
                marko.SetProperty("name", "marko");
                var matthias = graph.AddVertex(null);
                matthias.SetProperty("name", "matthias");
                var stephen = graph.AddVertex(null);
                stephen.SetProperty("name", "stephen");

                var edge = marko.AddEdge(null, "knows", stephen);
                edge.SetProperty("type", "tinkerpop");
                edge.SetProperty("weight", 1.0);
                edge = marko.AddEdge(null, "knows", matthias);
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
                Assert.AreEqual(
                    graph.Query().Has("weight", 1.0).Has("type", "tinkerpop").Edges().First().GetProperty("type"),
                    "tinkerpop");
                Assert.AreEqual(Count(graph.Query().Has("weight", 1.0).Has("type", "aurelius").Edges()), 0);

                Assert.AreEqual(graph.Query().Interval("weight", 0.0, 1.1).Edges().First().GetProperty("type"),
                                "tinkerpop");
                Assert.AreEqual(Count(graph.Query().Interval("weight", 0.0, 1.0).Edges()), 0);
            }
            finally
            {
                graph.Shutdown();
            }
        }
    }
}