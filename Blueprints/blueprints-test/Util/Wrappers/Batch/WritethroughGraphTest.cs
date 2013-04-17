using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Frontenac.Blueprints.Impls;
using Frontenac.Blueprints.Impls.TG;

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

    class WritethroughGraphTestImpl : GraphTest
    {
        public override Graph GenerateGraph()
        {
            return GenerateGraph("");
        }

        public override Graph GenerateGraph(string graphDirectoryName)
        {
            return new WritethroughGraph(new TinkerGraph());
        }
    }
}
