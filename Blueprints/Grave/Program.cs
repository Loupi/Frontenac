using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using Frontenac.Blueprints;
using Frontenac.Blueprints.Impls.TG;
using Frontenac.Grave.Entities;
using Frontenac.Grave.Geo;
using Frontenac.Gremlinq;

namespace Frontenac.Grave
{
    public class StaticGremlinqContextFactory : IGremlinqContextFactory
    {
        private readonly DictionaryAdapterProxyFactory _proxyFactory = new DictionaryAdapterProxyFactory();
        private readonly DictionaryTypeProvider _typeProvider;

        public StaticGremlinqContextFactory(IDictionary<int, Type> types)
        {
            _typeProvider = new DictionaryTypeProvider(DictionaryTypeProvider.DefaulTypePropertyName, types);
        }

        public GremlinqContext Create()
        {
            return new GremlinqContext(_typeProvider, _proxyFactory);
        }
    }

    public static class Program
    {
        public interface IChildren
        {
            bool IsAdopted { get; set; }
        }

        public interface IHeirChildren : IChildren
        {
            float Heirloom { get; set; }
        }

        public interface IJob
        {
            string Title { get; set; }
            string Domain { get; set; }
        }

        public interface IPerson
        {
            string Name { get; set; }
            int Age { get; set; }

            //A relation pointing to a father vertex. The edge is not typed, and it's in vertex is of type IPerson.
            IPerson Father { get; set; }

            //A relation pointing to a mother vertex. The edge is of type IChildren, and it's in vertex is of type IPerson.
            KeyValuePair<IChildren, IPerson> Mother { get; set; }
            
            //A relation pointing to children vertices. The edge is of type IChildren, and it's in vertex is of type IPerson
            IEnumerable<KeyValuePair<IChildren, IPerson>> Children { get; set; }
            
            //A relation pointing to job vertices. The edge is not typed, and it's in vertex is of type IJob
            IEnumerable<IJob> Job { get; set; }
        }

        private static void Test()
        {
            var graph = new TinkerGraph();

            //Add 3 persons vertex in the graph.
            var loupi = graph.AddVertex<IPerson>(person => { person.Name = "Loupi"; person.Age = 34; });
            var pierre = graph.AddVertex<IPerson>(person => { person.Name = "Pierre"; person.Age = 62; });
            var helene = graph.AddVertex<IPerson>(person => { person.Name = "Helene"; person.Age = 55; });

            //Add 2 jobs vertex in the graph
            var developer = graph.AddVertex<IJob>(job => { job.Title = "Developer"; job.Domain = "IT"; });
            var scrumMaster = graph.AddVertex<IJob>(job => { job.Title = "ScrumMaster"; job.Domain = "IT"; });
            
            loupi.AddEdge(person => person.Father, pierre);
            loupi.AddEdge(person => person.Mother, helene);

            loupi.AddEdge(person => person.Job, developer);
            loupi.AddEdge(person => person.Job, scrumMaster);

            pierre.AddEdge(person => person.Children, loupi);
            helene.AddEdge(person => person.Children, loupi, children => children.IsAdopted = false);
            helene.AddEdge(person => person.Children, loupi, (IHeirChildren heir) => heir.Heirloom = 125000.0f);
            
            var rawPerson = graph.V("Name", "Loupi").Single();
            var wrappedPerson = graph.V<IPerson, string>(p => p.Name, "Loupi").Single();

            var rawFather = rawPerson.Out("Father").Single();
            var father = wrappedPerson.Out(p => p.Father).Single();

            var rawChildrens = rawPerson.In("Children").ToArray();
            var childrens = wrappedPerson.In(p => p.Children).ToArray();

            var rawChildsOfFather = pierre.In("Father").ToArray();
            var childsOfFather = pierre.In(p => p.Father).ToArray();

            
            /*var saturn = graph.AddVertex<ITitan>(t => { t.Name = "Saturn"; t.Age = 10000; });
            var jupiter = graph.AddVertex<IGod>(t => { t.Name = "Jupiter"; t.Age = 5000; });*/
        }

        private static void Main()
        {
            GremlinqContext.ContextFactory = new StaticGremlinqContextFactory(
                new Dictionary<int, Type> //The types that are allowed to be proxied
                    {
                        {1, typeof (IAgedCharacter)},
                        {2, typeof (IBattle)},
                        {3, typeof (ICharacter)},
                        {4, typeof (IContributor)},
                        {5, typeof (IDemiGod)},
                        {6, typeof (IGod)},
                        {7, typeof (IHuman)},
                        {8, typeof (ILive)},
                        {9, typeof (ILocation)},
                        {10, typeof (IMonster)},
                        {11, typeof (INamedEntity)},
                        {12, typeof (ITitan)},
                        {13, typeof (IWeightedEntity)}
                    });
            
            Test();

            var graph = GraveFactory.CreateTransactionalGraph();
            try
            {
                if (!graph.V<ICharacter, string>(t => t.Name, "Saturn").Any())
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
                    .Single().Model.Name;

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
                    .Loop(t => t.In(u => u.Father).SingleOrDefault() , 2)
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

                var humanParent = parents
                    .OfType<IHuman>()
                    .Single();

                var battled = hercules
                    .Out(t => t.Battled)
                    .ToArray();

                var opponentDetails = battled
                    .Select(t => t.Element.Map())
                    .ToArray();

                var v2 = hercules
                    .OutE(t => t.Battled)
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