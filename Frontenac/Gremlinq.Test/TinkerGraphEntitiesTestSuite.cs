using Frontenac.Blueprints.Impls;
using Frontenac.Blueprints.Impls.TG;
using NUnit.Framework;

namespace Frontenac.Gremlinq.Test
{
    [TestFixture(Category = "TinkerGraphEntitiesTestSuite")]
    public class TinkerGraphEntitiesTestSuite : EntitiesTestSuite
    {
        [SetUp]
        public void SetUp()
        {
            DeleteDirectory(TinkerGraphTestImpl.GetThinkerGraphDirectory());
        }

        public TinkerGraphEntitiesTestSuite()
            : base(new GremlinqTinkerGraphTestImpl())
        {
        }

        public TinkerGraphEntitiesTestSuite(GraphTest graphTest)
            : base(graphTest)
        {
        }
    }
}