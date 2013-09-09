using NUnit.Framework;

namespace Frontenac.Blueprints.Util.IO.GraphSON
{
    [TestFixture(Category = "ElementPropertyConfigTest")]
    public class ElementPropertyConfigTest
    {
        [Test]
        public void ShouldExcludeBoth()
        {
            ElementPropertyConfig config = ElementPropertyConfig.ExcludeProperties(null, null);
            Assert.AreEqual(ElementPropertyConfig.ElementPropertiesRule.Exclude, config.VertexPropertiesRule);
            Assert.AreEqual(ElementPropertyConfig.ElementPropertiesRule.Exclude, config.EdgePropertiesRule);
        }

        [Test]
        public void ShouldIncludeBoth()
        {
            ElementPropertyConfig config = ElementPropertyConfig.IncludeProperties(null, null);
            Assert.AreEqual(ElementPropertyConfig.ElementPropertiesRule.Include, config.VertexPropertiesRule);
            Assert.AreEqual(ElementPropertyConfig.ElementPropertiesRule.Include, config.EdgePropertiesRule);
        }
    }
}
