using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.IO;
using System.Linq;
using System.Threading;
using Frontenac.Blueprints;
using Frontenac.Blueprints.Geo;
using Frontenac.Blueprints.Impls;
using Frontenac.Gremlinq.Test.Entities;
using NUnit.Framework;

namespace Frontenac.Gremlinq.Test
{
    public abstract class EntitiesTestSuite : TestSuite
    {
        protected EntitiesTestSuite(GraphTest graphTest)
            : base("EntitiesTestSuite", graphTest)
        {
        }

        static void CreateGraphOfTheGods(IKeyIndexableGraph graph)
        {
            Contract.Requires(graph != null);

            graph.CreateVertexIndex<INamedEntity, string>(t => t.Name);
            graph.CreateEdgeIndex<IBattle, GeoPoint>(t => t.Place);

            var sky = graph.AddVertex<ILocation>(t => t.Name = "Sky");
            var sea = graph.AddVertex<ILocation>(t => t.Name = "Sea");
            var tartarus = graph.AddVertex<ILocation>(t => t.Name = "Tartarus");

            var saturn = graph.AddVertex<ITitan>(t =>
                {
                    t.Name = "Saturn"; t.Age = 10000;
                });
            var jupiter = graph.AddVertex<IGod>(t => { t.Name = "Jupiter"; t.Age = 5000; });
            var neptune = graph.AddVertex<IGod>(t => { t.Name = "Neptune"; t.Age = 4500; });
            var pluto = graph.AddVertex<IGod>(t => { t.Name = "Pluto"; t.Age = 4000; });
            var hercules = graph.AddVertex<IDemiGod>(t => { t.Name = "Hercules"; t.Age = 30; });
            var alcmene = graph.AddVertex<IHuman>(t => { t.Name = "Alcmene"; t.Age = 45; });
            var nemean = graph.AddVertex<IMonster>(t => t.Name = "Nemean");
            var hydra = graph.AddVertex<IMonster>(t => t.Name = "Hydra");
            var cerberus = graph.AddVertex<IMonster>(t => t.Name = "Cerberus");

            jupiter.AddEdge(null, t => t.Father, saturn);
            jupiter.AddEdge(null, t => t.Lives, sky, t => t.Reason = "Loves fresh breezes.");
            jupiter.AddEdge(null, t => t.Brother, neptune);
            jupiter.AddEdge(null, t => t.Brother, pluto);

            neptune.AddEdge(null, t => t.Lives, sea, t => t.Reason = "Loves waves.");
            neptune.AddEdge(null, t => t.Brother, jupiter);
            neptune.AddEdge(null, t => t.Brother, pluto);

            hercules.AddEdge(null, t => t.Father, jupiter);
            hercules.AddEdge(null, t => t.Mother, alcmene);
            hercules.AddEdge(null, t => t.Battled, nemean, t => { t.Time = 1; t.Place = new GeoPoint(38.1, 23.7); });
            hercules.AddEdge(null, t => t.Battled, hydra, t => { t.Time = 2; t.Place = new GeoPoint(37.7, 23.9); });
            hercules.AddEdge(null, t => t.Battled, cerberus, t => { t.Time = 12; t.Place = new GeoPoint(39, 22); });

            pluto.AddEdge(null, t => t.Brother, jupiter);
            pluto.AddEdge(null, t => t.Brother, neptune);
            pluto.AddEdge(null, t => t.Lives, tartarus, t => t.Reason = "No fear of death");
            pluto.AddEdge(null, t => t.Pet, cerberus);

            cerberus.AddEdge(null, t => t.Lives, tartarus);
        }

        [Test]
        public void TestGraphOfTheGods()
        {
            var graph = (IKeyIndexableGraph)GraphTest.GenerateGraph();
            try
            {
                if (!graph.V<ICharacter, string>(t => t.Name, "Saturn").Any())
                {
                    CreateGraphOfTheGods(graph);
                }

                Thread.Sleep(1000);

                var saturn = graph
                    .V<ICharacter, string>(t => t.Name, "Saturn")
                    .Single();

                var jupiter = graph
                    .V<ICharacter, string>(t => t.Name, "Jupiter")
                    .Single();

                var both = jupiter.Both().ToArray();
                Assert.NotNull(both);
                var sIn = jupiter.In();
                Assert.NotNull(sIn);
                var sOut = jupiter.Out();
                Assert.NotNull(sOut);

                var map = saturn.Element.Map();
                Assert.NotNull(map);

                var fatherName = saturn
                    .In(t => t.Father)
                    .In(t => t.Father)
                    .Single().Model.Name;
                Assert.NotNull(fatherName);

                var eventsNearAthen = graph.Query<IBattle>()
                                           .Has(t => t.Place, new GeoCircle(37.97, 23.72, 50))
                                           .Edges()
                                           .ToArray();
                Assert.NotNull(eventsNearAthen);
                /*var opps = eventsNearAthen[0].Model.Opponent;
                var oopps = eventsNearAthen[0].Model.BothOpponent.ToArray();
                var vvv = eventsNearAthen[0].Model.BothOpponent.ElementAt(0);
                var vvv1 = eventsNearAthen[0].Model.BothOpponent.ElementAt(1);*/

                var opponents = eventsNearAthen
                    .Select(t => new[]
                        {
                            t.Out(u => u.Opponent).Model.Name,
                            t.In(u => u.Opponent).Model.Name
                        })
                    .ToArray();
                Assert.NotNull(opponents);

                var hercules = saturn
                    .Loop(t => t.In(u => u.Father).SingleOrDefault(), 2)
                    .Single();

                var parents = hercules
                    .Out(t => t.Father, t => t.Mother)
                    .ToArray();

                var parentNames = parents
                    .Select(t => t.Model.Name)
                    .ToArray();
                Assert.NotNull(parentNames);

                var parentTypes = parents
                    .Select(t => t.Type())
                    .ToArray();
                Assert.NotNull(parentTypes);

                var humanParent = parents
                    .OfType<IHuman>()
                    .Single();
                Assert.NotNull(humanParent);

                var battled = hercules
                    .Out(t => t.Battled)
                    .ToArray();

                var opponentDetails = battled
                    .Select(t => t.Element.Map())
                    .ToArray();
                Assert.NotNull(opponentDetails);

                var v2 = hercules
                    .OutE(t => t.Battled)
                    .Where(t => t.Model.Time > 1)
                    .Select(t => t.In(u => u.Opponent).Model.Name)
                    .ToArray();
                Assert.NotNull(v2);
            }
            finally
            {
                graph.Shutdown();
            }
        }

        [Test]
        public void TestFamily()
        {
            var graph = GraphTest.GenerateGraph();
            try
            {
                //Add 3 persons vertex in the InnerGraph.
                var loupi = graph.AddVertex<IPerson>(person => { person.Name = "Loupi"; person.Age = 34; }).Model;
                var pierre = graph.AddVertex<IPerson>(person => { person.Name = "Pierre"; person.Age = 62; }).Model;
                var helene = graph.AddVertex<IPerson>(person => { person.Name = "Helene"; person.Age = 55; }).Model;
                var claude = graph.AddVertex<IPerson>(person => { person.Name = "Claude"; person.Age = 57; }).Model;
                var mimi = graph.AddVertex<IPerson>(person => { person.Name = "Mimi"; person.Age = 3; }).Model;

                //Add 2 jobs vertex in the InnerGraph
                var developer = graph.AddVertex<IJob>(job => { job.Title = "Developer"; job.Domain = "IT"; }).Model;
                var scrumMaster = graph.AddVertex<IJob>(job => { job.Title = "ScrumMaster"; job.Domain = "IT"; }).Model;

                //Loupi is both a developer and a scrummaster.
                loupi.Job.Add(developer);
                loupi.Job.Add(scrumMaster);

                //Helene and Pierre are Loupi's parents. Note that they are divorced.
                loupi.Father = pierre;
                loupi.Mother = new KeyValuePair<IChildren, IPerson>(null, helene);
                pierre.Children.Add(loupi);
                helene.Children.Add(loupi, (IHeirChildren heir) => heir.Heirloom = 125000.0f);

                //Helene is now with Claude, and they adopted Mimi.
                mimi.Father = claude;
                mimi.Mother = new KeyValuePair<IChildren, IPerson>(null, helene);
                claude.Children.Add(mimi, children => children.IsAdopted = true);
                helene.Children.Add(graph.Transient<IChildren>(children => children.IsAdopted = true), mimi);

                var allDevelopers = developer.Persons.ToList();
                var heleneChildrenIds = helene.Children.Select(pair => pair.Value.Id).ToList();
                Assert.NotNull(allDevelopers);
                Assert.NotNull(heleneChildrenIds);
                var mimiParents = mimi.Parents.ToList();
                var loupiParents = loupi.Parents.ToList();
                var intersectedParents = loupi.WrappedParents.Intersect(mimi.WrappedParents).ToList();
                var joinedParents = mimiParents.Join(loupiParents, person => person.Id, person => person.Id, (person, person1) => person).ToList();
                Assert.NotNull(intersectedParents);
                Assert.NotNull(joinedParents);
            }
            finally
            {
                graph.Shutdown();
            }
        }

        [Test]
        public void LibImseti()
        {
            const string inputDataDir = "d:\\LibImseti";

            var graph = (IKeyIndexableGraph)GraphTest.GenerateTransactionalGraph();
            var idx = (IIndexableGraph)GraphTest.GenerateTransactionalGraph();
            var trans = (ITransactionalGraph) graph;
            try
            {
                graph.CreateVertexIndex<IRating, int>(t => t.Rating);
                idx.CreateIndex("frontenac_gremlinq", typeof(IVertex));
                trans.Commit();

                int id = 0;

                StopWatch();
                using (var stream = File.OpenRead(Path.Combine(inputDataDir, "gender.dat")))
                {
                    using (var file = new StreamReader(stream))
                    {
                        string line;
                        var lineNumber = 0;

                        while ((line = file.ReadLine()) != null)
                        {
                            if (lineNumber % 10000 == 0)
                            {
                                Console.WriteLine("Parsing user # {0}", lineNumber);
                                trans.Commit();
                            }
                                

                            lineNumber++;
                            var fields = line.Split(',');
                            id = int.Parse(fields[0]);
                            var gender = fields[1][0] == 'M' ? Gender.Male : fields[1][0] == 'F' ? Gender.Female : Gender.Unknown;
                            graph.AddVertex<IUser>(id, user => user.Gender = gender);
                        }

                        PrintPerformance(graph.ToString(), lineNumber, "Done importing users", StopWatch());
                    }
                }

                trans.Commit();

                using (var stream = File.OpenRead(Path.Combine(inputDataDir, "ratings.dat")))
                {
                    using (var file = new StreamReader(stream))
                    {
                        string line;
                        var lineNumber = 0;
                        IVertex<IUser> rater = null;
                        var priorRaterId = -1;
                        var ratingVertices = new Dictionary<int, IVertex<IRating>>();

                        while ((line = file.ReadLine()) != null)
                        {
                            lineNumber++;

                            if (lineNumber % 10000 == 0)
                            {
                                Console.WriteLine("Parsing rating # {0}", lineNumber);
                                trans.Commit();
                            }

                            var fields = line.Split(',');
                            var raterId = int.Parse(fields[0]);
                            if (raterId != priorRaterId)
                                rater = graph.V<IUser>(raterId);
                            Assert.NotNull(rater);
                            priorRaterId = raterId;
                            var ratedId = int.Parse(fields[1]);
                            var ratingValue = int.Parse(fields[2]);

                            IVertex<IRating> rating;
                            if (!ratingVertices.TryGetValue(ratingValue, out rating))
                            {
                                rating = graph.AddVertex<IRating>(++id, r => r.Rating = ratingValue);
                                ratingVertices.Add(ratingValue, rating);
                            }              

                            var rated = graph.V<IUser>(ratedId);
                            rater.AddEdge(null, t => t.Rated, rated, r => r.Rating = ratingValue);
                            rating.AddEdge(null, t => t.RatingOf, rated);

                            if (lineNumber >= 10000000) // remove this condition if you have time to wait a while and you have at least 16GB of RAM memory
                                break;
                        }
                        PrintPerformance(graph.ToString(), lineNumber, "Done importing ratings", StopWatch());
                    }
                }

                trans.Commit();

                var ratingsVertexEnum = graph.VerticesOfType<IRating>().OrderByDescending(t => t.Model.Rating).ToArray();

            }
            finally
            {
                graph.Shutdown();
            }
        }
    }
}