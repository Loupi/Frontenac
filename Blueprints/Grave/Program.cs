using System.Diagnostics.Contracts;
using System.Linq;
using Frontenac.Blueprints;
using Frontenac.Grave.Entities;
using Frontenac.Grave.Geo;
using Frontenac.Gremlinq;

namespace Frontenac.Grave
{
    public static class Program
    {
        private static void Main()
        {
            var graph = GraveFactory.CreateTransactionalGraph();
            try
            {
                if (!graph.GetVertices().Any())
                {
                    CreateGraphOfTheGods(graph);
                    graph.Commit();
                }

                var saturn = graph
                    .V<ICharacter, string>(t => t.Name, "Saturn")
                    .Single();

                var jupiter = graph
                    .V<ICharacter, string>(t => t.Name, "Jupiter")
                    .Single();

                var both = jupiter.Both().ToArray();
                var sIn = jupiter.In();
                var sOut = jupiter.Out();
                
                var map = saturn.Element.Map();

                var fatherName = saturn
                    .In(t => t.Father)
                    .In(t => t.Father)
                    .Select(t => t.Model.Name)
                    .Single();

                var eventsNearAthen = graph.Query<IBattle>()
                    .Has(t => t.Place, new GeoCircle(37.97, 23.72, 50))
                    .Edges()
                    .ToArray();
                
                var opponents = eventsNearAthen
                    .Select(t => new[]
                        {
                            t.Out(u => u.Opponent).Model.Name,
                            t.In(u => u.Opponent).Model.Name
                        })
                    .ToArray();

                var hercules = saturn
                    .Loop(t => t.In(u => u.Father) , 2)
                    .Single();

                var parents = hercules
                    .Out(t => t.Father, t => t.Mother)
                    .ToArray();

                var parentNames = parents
                    .Select(t => t.Model.Name)
                    .ToArray();

                var parentTypes = parents
                    .Select(t => t.Type())
                    .ToArray();

                var battled = hercules
                    .Out(t => t.Battled)
                    .ToArray();

                var opponentDetails = battled
                    .Select(t => t.Element.ToDictionary(u => u.Key, u => u.Value))
                    .ToArray();

                var v2 = hercules
                    .Out(t => t.Battled)
                    .Where(t => t.Model.Time > 1)
                    .Select(t => t.In(u => u.Opponent).Model.Name)
                    .ToArray();
            }
            finally
            {
                graph.Shutdown();
                GraveFactory.Release();
            }
        }

        public static void CreateGraphOfTheGods(IKeyIndexableGraph graph)
        {
            Contract.Requires(graph != null);

            graph.CreateVertexIndex<INamedEntity, string>(t => t.Name);
            graph.CreateEdgeIndex<IBattle, GeoPoint>(t => t.Place);

            var sky = graph.AddVertex<ILocation>(t => t.Name = "Sky");
            var sea = graph.AddVertex<ILocation>(t => t.Name = "Sea");
            var tartarus = graph.AddVertex<ILocation>(t => t.Name = "Tartarus");

            var saturn = graph.AddVertex<ITitan>(t => { t.Name = "Saturn"; t.Age = 10000; });
            var jupiter = graph.AddVertex<IGod>(t => { t.Name = "Jupiter"; t.Age = 5000; });
            var neptune = graph.AddVertex<IGod>(t => { t.Name = "Neptune"; t.Age = 4500; });
            var pluto = graph.AddVertex<IGod>(t => { t.Name = "Pluto"; t.Age = 4000; });
            var hercules = graph.AddVertex<IDemiGod>(t => { t.Name = "Hercules"; t.Age = 30; });          
            var alcmene = graph.AddVertex<IHuman>(t => { t.Name = "Alcmene"; t.Age = 45; });
            var nemean = graph.AddVertex<IMonster>(t => t.Name = "Nemean");
            var hydra = graph.AddVertex<IMonster>(t => t.Name = "Hydra");
            var cerberus = graph.AddVertex<IMonster>(t => t.Name = "Cerberus");
            
            jupiter.AddEdge(t => t.Father, saturn);
            jupiter.AddEdge(t => t.Lives, sky, t => t.Reason = "Loves fresh breezes.");
            jupiter.AddEdge(t => t.Brother, neptune);
            jupiter.AddEdge(t => t.Brother, pluto);

            neptune.AddEdge(t => t.Lives, sea, t => t.Reason = "Loves waves.");
            neptune.AddEdge(t => t.Brother, jupiter);
            neptune.AddEdge(t => t.Brother, pluto);

            hercules.AddEdge(t => t.Father, jupiter);
            hercules.AddEdge(t => t.Mother, alcmene);
            hercules.AddEdge(t => t.Battled, nemean, t => { t.Time = 1; t.Place = new GeoPoint(38.1, 23.7); });
            hercules.AddEdge(t => t.Battled, hydra, t => { t.Time = 2; t.Place = new GeoPoint(37.7, 23.9); });
            hercules.AddEdge(t => t.Battled, cerberus, t => { t.Time = 12; t.Place = new GeoPoint(39, 22); });

            pluto.AddEdge(t => t.Brother, jupiter);
            pluto.AddEdge(t => t.Brother, neptune);
            pluto.AddEdge(t => t.Lives, tartarus, t => t.Reason = "No fear of death");
            pluto.AddEdge(t => t.Pet, cerberus);

            cerberus.AddEdge(t => t.Lives, tartarus);
        }
    }
}