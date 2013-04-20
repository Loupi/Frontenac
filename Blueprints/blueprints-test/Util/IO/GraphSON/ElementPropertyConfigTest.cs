using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Frontenac.Blueprints.Util.IO.GraphSON
{
    [TestFixture(Category = "ElementPropertyConfigTest")]
    public class ElementPropertyConfigTest
    {
        [Test]
        public void shouldExcludeBoth()
        {
            ElementPropertyConfig config = ElementPropertyConfig.ExcludeProperties(null, null);
            Assert.AreEqual(ElementPropertyConfig.ElementPropertiesRule.EXCLUDE, config.getVertexPropertiesRule());
            Assert.AreEqual(ElementPropertyConfig.ElementPropertiesRule.EXCLUDE, config.getEdgePropertiesRule());
        }

        [Test]
        public void shouldIncludeBoth()
        {
            ElementPropertyConfig config = ElementPropertyConfig.IncludeProperties(null, null);
            Assert.AreEqual(ElementPropertyConfig.ElementPropertiesRule.INCLUDE, config.getVertexPropertiesRule());
            Assert.AreEqual(ElementPropertyConfig.ElementPropertiesRule.INCLUDE, config.getEdgePropertiesRule());
        }
    }
}
