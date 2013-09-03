using NUnit.Framework;

namespace Frontenac.Blueprints.Impls.VG
{
  //Tests IgnoreIdVelocityGraph using the standard test suite.
  public class IgnoreIdVelocityGraphTestImpl : VelocityGraphTestImpl
  {
  }

  [TestFixture(Category = "IgnoreIdVelocityGraphTestSuite")]
  public class IgnoreIdVelocityGraphTestSuite : VelocityGraphGraphTestSuite
  {
    public IgnoreIdVelocityGraphTestSuite()
      : base(new IgnoreIdVelocityGraphTestImpl())
    {
    }
  }

  [TestFixture(Category = "IgnoreIdVelocityGraphTestSuite")]
  public class IgnoreIdVelocityGraphVertexTestSuite : VelocityGraphVertexTestSuite
  {
    public IgnoreIdVelocityGraphVertexTestSuite()
      : base(new IgnoreIdVelocityGraphTestImpl())
    {
    }
  }

  [TestFixture(Category = "IgnoreIdVelocityGraphTestSuite")]
  public class IgnoreIdVelocityGraphEdgeTestSuite : VelocityGraphEdgeTestSuite
  {
    public IgnoreIdVelocityGraphEdgeTestSuite()
      : base(new IgnoreIdVelocityGraphTestImpl())
    {
    }
  }

  [TestFixture(Category = "IgnoreIdVelocityGraphTestSuite")]
  public class IgnoreIdVelocityGraphGmlReaderTestSuite : VelocityGraphGmlReaderTestSuite
  {
    public IgnoreIdVelocityGraphGmlReaderTestSuite()
      : base(new IgnoreIdVelocityGraphTestImpl())
    {
    }
  }

  [TestFixture(Category = "IgnoreIdVelocityGraphTestSuite")]
  public class IgnoreIdVelocityGraphGraphMlReaderTestSuite : VelocityGraphGraphMlReaderTestSuite
  {
    public IgnoreIdVelocityGraphGraphMlReaderTestSuite()
      : base(new IgnoreIdVelocityGraphTestImpl())
    {
    }
  }

  [TestFixture(Category = "IgnoreIdVelocityGraphTestSuite")]
  public class IgnoreIdVelocityGraphGraphSonReaderTestSuite : VelocityGraphGraphSonReaderTestSuite
  {
    public IgnoreIdVelocityGraphGraphSonReaderTestSuite()
      : base(new IgnoreIdVelocityGraphTestImpl())
    {
    }
  }

  [TestFixture(Category = "IgnoreIdVelocityGraphTestSuite")]
  public class IgnoreIdVelocityGraphTestGeneral : VelocityGraphTestGeneral
  {
    public IgnoreIdVelocityGraphTestGeneral()
      : base(new IgnoreIdVelocityGraphTestImpl())
    {
    }
  }
}
