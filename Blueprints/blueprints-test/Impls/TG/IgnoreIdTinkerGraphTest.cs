using Frontenac.Blueprints.Util.IO.GML;
using Frontenac.Blueprints.Util.IO.GraphML;
using Frontenac.Blueprints.Util.IO.GraphSON;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Frontenac.Blueprints.Impls.TG
{
    //Tests IgnoreIdTinkerGraph using the standard test suite.
    public class IgnoreIdTinkerGraphTestImpl : TinkerGraphTestImpl
    {
        public override Graph generateGraph()
        {
            return new IgnoreIdTinkerGraph(IgnoreIdTinkerGraphTestImpl.getThinkerGraphDirectory());
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
    public class IgnoreIdTinkerGraphGMLReaderTestSuite : TinkerGraphGMLReaderTestSuite
    {
        public IgnoreIdTinkerGraphGMLReaderTestSuite()
            : base(new IgnoreIdTinkerGraphTestImpl())
        {
        }
    }

    [TestFixture(Category = "IgnoreIdTinkerGraphTestSuite")]
    public class IgnoreIdTinkerGraphGraphMLReaderTestSuite : TinkerGraphGraphMLReaderTestSuite
    {
        public IgnoreIdTinkerGraphGraphMLReaderTestSuite()
            : base(new IgnoreIdTinkerGraphTestImpl())
        {
        }
    }

    [TestFixture(Category = "IgnoreIdTinkerGraphTestSuite")]
    public class IgnoreIdTinkerGraphGraphSONReaderTestSuite : TinkerGraphGraphSONReaderTestSuite
    {
        public IgnoreIdTinkerGraphGraphSONReaderTestSuite()
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
