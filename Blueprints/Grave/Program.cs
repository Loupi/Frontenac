using System.Collections.Generic;
using System.Linq;
using Frontenac.Blueprints;
using Frontenac.Blueprints.Util;
using Grave.Geo;

namespace Grave
{
    public static class Program
    {
        private static void Main()
        {
            var g = GraveFactory.CreateGraph();
            try
            {
                if (!g.GetIndexedKeys(typeof (IVertex)).Contains("name"))
                {
                    g.CreateKeyIndex("name", typeof(IVertex));
                    g.CreateKeyIndex("place", typeof(IEdge));
                }

                if (!g.GetVertices().Any())
                    CreateGraphOfTheGods(g);

                var saturn = g.V("name", "saturn")
                              .Single();

                
                var nameFromModel = saturn.As<IGod>().name;
                
                var map = saturn.Map();

                var fatherName = saturn.In("father")
                                       .In("father")
                                       .P<string>("name")
                                       .Single();
                
                var eventsNearAthen = g.Query()
                    .Has("place", Compare.Equal, new GeoCircle(37.97, 23.72, 50))
                    .Edges()
                    .ToArray();

                var opponents = eventsNearAthen
                    .Select(t => new[]
                        {
                            t.GetVertex(Direction.Out)["name"],
                            t.GetVertex(Direction.In)["name"]
                        })
                    .ToArray();

                var hercules = saturn.Loop(t => t.In("father"), 2).Single();

                var parents = hercules.GetVertices(Direction.Out, "father", "mother").ToArray();

                var parentNames = parents.Select(t => t["name"])
                                         .ToArray();

                var parentTypes = parents.Select(t => t["type"])
                                         .ToArray();

                var battled = hercules.GetVertices(Direction.Out, "battled").ToArray();

                var opponentDetails = battled
                    .Select(t => t.ToDictionary(u => u.Key, u => u.Value))
                    .ToArray();

                var v2 = hercules.GetEdges(Direction.Out, "battled")
                                 //.Where(t => t.P<God>((m, e) => e.P<>()
                                 //    element.P<God>()) )
                                 .Where(t => t.P<int>("time") > 1)
                                 .Select(t => t.GetVertex(Direction.In)["name"])
                                 .ToArray();
            }
            finally
            {
                g.Shutdown();
                GraveFactory.Release();
            }
        }

        public interface IGod
        {
            string name { get; set; }
            string type { get; set; }
            int age { get; set; }
        }

        private static void CreateGraphOfTheGods(IGraph graph)
        {
            var saturn = graph.AddVertex(null);
            saturn.Add("name", "saturn");
            saturn.Add("age", 10000);
            saturn.Add("type", "titan");

            var sky = graph.AddVertex(null);
            sky.CopyTo(new[]
                {
                    new KeyValuePair<string, object>("name", "sky"),
                    new KeyValuePair<string, object>("type", "location")
                }, 0);

            ElementHelper.SetProperties(sky, "name", "sky", "type", "location");

            var sea = graph.AddVertex(null);
            ElementHelper.SetProperties(sea, "name", "sea", "type", "location");

            var jupiter = graph.AddVertex(null);
            ElementHelper.SetProperties(jupiter, "name", "jupiter", "age", 5000, "type", "god");

            var neptune = graph.AddVertex(null);
            ElementHelper.SetProperties(neptune, "name", "neptune", "age", 4500, "type", "god");

            var hercules = graph.AddVertex(null);
            ElementHelper.SetProperties(hercules, "name", "hercules", "age", 30, "type", "demigod");

            var alcmene = graph.AddVertex(null);
            ElementHelper.SetProperties(alcmene, "name", "alcmene", "age", 45, "type", "human");

            var pluto = graph.AddVertex(null);
            ElementHelper.SetProperties(pluto, "name", "pluto", "age", 4000, "type", "god");

            var nemean = graph.AddVertex(null);
            ElementHelper.SetProperties(nemean, "name", "nemean", "type", "monster");

            var hydra = graph.AddVertex(null);
            ElementHelper.SetProperties(hydra, "name", "hydra", "type", "monster");

            var cerberus = graph.AddVertex(null);
            ElementHelper.SetProperties(cerberus, "name", "cerberus", "type", "monster");

            var tartarus = graph.AddVertex(null);
            ElementHelper.SetProperties(tartarus, "name", "tartarus", "type", "location");

            // edges

            jupiter.AddEdge("father", saturn);
            jupiter.AddEdge("lives", sky).SetProperty("reason", "loves fresh breezes");
            jupiter.AddEdge("brother", neptune);
            jupiter.AddEdge("brother", pluto);

            neptune.AddEdge("lives", sea).SetProperty("reason", "loves waves");
            neptune.AddEdge("brother", jupiter);
            neptune.AddEdge("brother", pluto);

            hercules.AddEdge("father", jupiter);
            hercules.AddEdge("mother", alcmene);

            ElementHelper.SetProperties(hercules.AddEdge("battled", nemean), "time", 1, "place",
                                        new GeoPoint(38.1, 23.7));
            ElementHelper.SetProperties(hercules.AddEdge("battled", hydra), "time", 2, "place",
                                        new GeoPoint(37.7, 23.9));
            ElementHelper.SetProperties(hercules.AddEdge("battled", cerberus), "time", 12, "place",
                                        new GeoPoint(39, 22));

            pluto.AddEdge("brother", jupiter);
            pluto.AddEdge("brother", neptune);
            pluto.AddEdge("lives", tartarus).SetProperty("reason", "no fear of death");
            pluto.AddEdge("pet", cerberus);

            cerberus.AddEdge("lives", tartarus);
        }
    }
}