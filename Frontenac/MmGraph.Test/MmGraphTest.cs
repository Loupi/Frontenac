using System;
using System.IO;
using System.Runtime.CompilerServices;
using Frontenac.Blueprints;
using Frontenac.Blueprints.Impls;
using Frontenac.CastleWindsor;
using Frontenac.Infrastructure;
using NUnit.Framework;

namespace MmGraph.Test
{
    public class MmGraphTest : GraphTest
    {
        public IGraphFactory Factory { get; set; }

        public override IGraph GenerateGraph()
        {
            return GenerateGraph("MmGraph");
        }

        public override IGraph GenerateGraph(string graphDirectoryName)
        {
            return Factory.Create<IGraph>();
        }

        public override ITransactionalGraph GenerateTransactionalGraph()
        {
            throw new NotSupportedException();
        }

        public override ITransactionalGraph GenerateTransactionalGraph(string graphDirectoryName)
        {
            throw new NotSupportedException();
        }

        public static string GetGraphDirectory()
        {
            return Path.Combine(TestContext.CurrentContext.TestDirectory, "MmGraph");
        }
    }

    public class MmGraphTestSuite
    {
        public static ConditionalWeakTable<MmGraphTestSuite, CastleWindsorContainer> Containers { get; } =
            new ConditionalWeakTable<MmGraphTestSuite, CastleWindsorContainer>();

        public void SetUp(GraphTest graphTest)
        {
            Directory.SetCurrentDirectory(TestContext.CurrentContext.TestDirectory);
            var directory = MmGraphTest.GetGraphDirectory();
            DeleteDirectory(directory);
            Directory.CreateDirectory(directory);
            Directory.SetCurrentDirectory(directory);

            var container = new CastleWindsorContainer();
            container.SetupMmGraph();
            Factory = container.Resolve<IGraphFactory>();

            Containers.Add(this, container);
            ((MmGraphTest)graphTest).Factory = Factory;
        }

        private static void DeleteDirectory(string directory)
        {
            if (Directory.Exists(directory))
                Directory.Delete(directory, true);
        }

        public void TearDown()
        {
            CastleWindsorContainer container;
            if (Containers.TryGetValue(this, out container))
            {
                container.Release(Factory);
                Factory.Dispose();
                container.Dispose();
                Containers.Remove(this);
            }

            Directory.SetCurrentDirectory(TestContext.CurrentContext.TestDirectory);
            DeleteDirectory(MmGraphTest.GetGraphDirectory());
        }

        public IGraphFactory Factory { get; private set; }
    }

    [TestFixture(Category = "MmGraphGraphTestSuite")]
    public class MmGraphGraphTestSuite : GraphTestSuite
    {
        private readonly MmGraphTestSuite _suite = new MmGraphTestSuite();

        [SetUp]
        public void SetUp()
        {
            _suite.SetUp(GraphTest);
        }

        [TearDown]
        public void TearDown()
        {
            _suite.TearDown();
        }

        public MmGraphGraphTestSuite()
            : base(new MmGraphTest())
        {
        }

        public MmGraphGraphTestSuite(GraphTest graphTest)
            : base(graphTest)
        {
        }
    }

    [TestFixture(Category = "MmGraphGraphTestSuite")]
    public class MmGraphVertexTestSuite : VertexTestSuite
    {
        private readonly MmGraphTestSuite _suite = new MmGraphTestSuite();

        [SetUp]
        public void SetUp()
        {
            _suite.SetUp(GraphTest);
        }

        [TearDown]
        public void TearDown()
        {
            _suite.TearDown();
        }

        public MmGraphVertexTestSuite()
            : base(new MmGraphTest())
        {
        }

        public MmGraphVertexTestSuite(GraphTest graphTest)
            : base(graphTest)
        {
        }
    }
}
