using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Frontenac.Blueprints.Impls.TG
{
    /// <summary>
    /// This is class is an in-memory variant of TinkerGraph that ignores the supplied ids
    /// and instead uses its own internal id scheme.
    /// This is meant to be used for testing only.
    /// </summary>
    public class IgnoreIdTinkerGraph : TinkerGraph
    {
        public IgnoreIdTinkerGraph()
        {

        }

        public IgnoreIdTinkerGraph(string directory)
            : base(directory)
        {

        }

        public override Features getFeatures()
        {
            Features f = base.getFeatures().copyFeatures();
            f.ignoresSuppliedIds = true;
            return f;
        }

        public override Vertex addVertex(object id)
        {
            return base.addVertex(null);
        }

        public override Edge addEdge(object id, Vertex outVertex, Vertex inVertex, string label)
        {
            return base.addEdge(null, outVertex, inVertex, label);
        }
    }
}
