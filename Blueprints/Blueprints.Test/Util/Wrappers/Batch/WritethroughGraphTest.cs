using Frontenac.Blueprints.Impls;
using Frontenac.Blueprints.Impls.TG;
using Frontenac.Blueprints.Util.IO.GML;
using Frontenac.Blueprints.Util.IO.GraphML;
using Frontenac.Blueprints.Util.IO.GraphSON;
using NUnit.Framework;

namespace Frontenac.Blueprints.Util.Wrappers.Batch
{
    [TestFixture(Category = "WritethroughGraphTestSuite")]
    public class WritethroughGraphVertexTestSuite : VertexTestSuite
    {
        public WritethroughGraphVertexTestSuite()
            : base(new WritethroughGraphTestImpl())
        {
        }
    }

    [TestFixture(Category = "WritethroughGraphTestSuite")]
    public class WritethroughGraphEdgeTestSuite : EdgeTestSuite
    {
        public WritethroughGraphEdgeTestSuite()
            : base(new WritethroughGraphTestImpl())
        {
        }
    }

    [TestFixture(Category = "WritethroughGraphTestSuite")]
    public class WritethroughGraphGraphTestSuite : GraphTestSuite
    {
        public WritethroughGraphGraphTestSuite()
            : base(new WritethroughGraphTestImpl())
        {
        }
    }

    [TestFixture(Category = "WritethroughGraphTestSuite")]
    public class WritethroughGraphGraphMlReaderTestSuite : GraphMlReaderTestSuite
    {
        public WritethroughGraphGraphMlReaderTestSuite()
            : base(new WritethroughGraphTestImpl())
        {
        }
    }

    [TestFixture(Category = "WritethroughGraphTestSuite")]
    public class WritethroughGraphGmlReaderTestSuite : GmlReaderTestSuite
    {
        public WritethroughGraphGmlReaderTestSuite()
            : base(new WritethroughGraphTestImpl())
        {
        }
    }

    [TestFixture(Category = "WritethroughGraphTestSuite")]
    public class WritethroughGraphGraphSonReaderTestSuite : GraphSonReaderTestSuite
    {
        public WritethroughGraphGraphSonReaderTestSuite()
            : base(new WritethroughGraphTestImpl())
        {
        }
    }

    internal class WritethroughGraphTestImpl : GraphTest
    {
        public override IGraph GenerateGraph()
        {
            return GenerateGraph("");
        }

        public override IGraph GenerateGraph(string graphDirectoryName)
        {
            return new WritethroughGraph(new TinkerGraph());
        }
    }
}