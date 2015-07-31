using Frontenac.Blueprints.Impls;
using Frontenac.Blueprints.Impls.TG;
using NUnit.Framework;

namespace Frontenac.Gremlinq.Test
{
    [TestFixture(Category = "TinkerGraphGremlinDocsTestSuite")]
    public class TinkerGraphGremlinDocsTestSuite : GremlinDocsTestSuite
    {
        [SetUp]
        public void SetUp()
        {
            DeleteDirectory(TinkerGraphTestImpl.GetThinkerGraphDirectory());
        }

        public TinkerGraphGremlinDocsTestSuite()
            : base(new GremlinqTinkerGraphTestImpl())
        {
        }

        public TinkerGraphGremlinDocsTestSuite(GraphTest graphTest)
            : base(graphTest)
        {
        }
    }
}