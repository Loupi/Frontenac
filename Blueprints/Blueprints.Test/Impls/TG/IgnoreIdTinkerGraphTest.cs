using NUnit.Framework;

namespace Frontenac.Blueprints.Impls.TG
{
    //Tests IgnoreIdTinkerGraph using the standard test suite.
    public class IgnoreIdTinkerGraphTestImpl : TinkerGraphTestImpl
    {
        public override IGraph GenerateGraph()
        {
            return new IgnoreIdTinkerGraph(GetThinkerGraphDirectory());
        }
    }

    [TestFixture(Category = "IgnoreIdTinkerGraphTestSuite")]
    public class IgnoreIdTinkerGraphTestSuite : TinkerGraphGraphTestSuite
    {
        public IgnoreIdTinkerGraphTestSuite()
            : base(new IgnoreIdTinkerGraphTestImpl())
        {
        }
    }

    [TestFixture(Category = "IgnoreIdTinkerGraphTestSuite")]
    public class IgnoreIdTinkerGraphVertexTestSuite : TinkerGraphVertexTestSuite
    {
        public IgnoreIdTinkerGraphVertexTestSuite()
            : base(new IgnoreIdTinkerGraphTestImpl())
        {
        }
    }

    [TestFixture(Category = "IgnoreIdTinkerGraphTestSuite")]
    public class IgnoreIdTinkerGraphEdgeTestSuite : TinkerGraphEdgeTestSuite
    {
        public IgnoreIdTinkerGraphEdgeTestSuite()
            : base(new IgnoreIdTinkerGraphTestImpl())
        {
        }
    }

    [TestFixture(Category = "IgnoreIdTinkerGraphTestSuite")]
    public class IgnoreIdTinkerGraphIndexableGraphTestSuite : TinkerGraphIndexableGraphTestSuite
    {
        public IgnoreIdTinkerGraphIndexableGraphTestSuite()
            : base(new IgnoreIdTinkerGraphTestImpl())
        {
        }
    }

    [TestFixture(Category = "IgnoreIdTinkerGraphTestSuite")]
    public class IgnoreIdTinkerGraphIndexTestSuite : TinkerGraphIndexTestSuite
    {
        public IgnoreIdTinkerGraphIndexTestSuite()
            : base(new IgnoreIdTinkerGraphTestImpl())
        {
        }
    }

    [TestFixture(Category = "IgnoreIdTinkerGraphTestSuite")]
    public class IgnoreIdTinkerGraphKeyIndexableGraphTestSuite : TinkerGraphKeyIndexableGraphTestSuite
    {
        public IgnoreIdTinkerGraphKeyIndexableGraphTestSuite()
            : base(new IgnoreIdTinkerGraphTestImpl())
        {
        }
    }

    [TestFixture(Category = "IgnoreIdTinkerGraphTestSuite")]
    public class IgnoreIdTinkerGraphGmlReaderTestSuite : TinkerGraphGmlReaderTestSuite
    {
        public IgnoreIdTinkerGraphGmlReaderTestSuite()
            : base(new IgnoreIdTinkerGraphTestImpl())
        {
        }
    }

    [TestFixture(Category = "IgnoreIdTinkerGraphTestSuite")]
    public class IgnoreIdTinkerGraphGraphMlReaderTestSuite : TinkerGraphGraphMlReaderTestSuite
    {
        public IgnoreIdTinkerGraphGraphMlReaderTestSuite()
            : base(new IgnoreIdTinkerGraphTestImpl())
        {
        }
    }

    [TestFixture(Category = "IgnoreIdTinkerGraphTestSuite")]
    public class IgnoreIdTinkerGraphGraphSonReaderTestSuite : TinkerGraphGraphSonReaderTestSuite
    {
        public IgnoreIdTinkerGraphGraphSonReaderTestSuite()
            : base(new IgnoreIdTinkerGraphTestImpl())
        {
        }
    }

    [TestFixture(Category = "IgnoreIdTinkerGraphTestSuite")]
    public class IgnoreIdTinkerGraphTestGeneral : TinkerGraphTestGeneral
    {
        public IgnoreIdTinkerGraphTestGeneral()
            : base(new IgnoreIdTinkerGraphTestImpl())
        {
        }
    }
}