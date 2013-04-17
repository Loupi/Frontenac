using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Frontenac.Blueprints.Impls
{
    public abstract class GraphTest : BaseTest
    {
        public abstract Graph GenerateGraph();

        public abstract Graph GenerateGraph(string graphDirectoryName);
    }
}
