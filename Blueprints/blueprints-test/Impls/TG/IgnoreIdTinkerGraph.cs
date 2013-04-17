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

        public override Features GetFeatures()
        {
            Features f = base.GetFeatures().CopyFeatures();
            f.IgnoresSuppliedIds = true;
            return f;
        }

        public override Vertex AddVertex(object id)
        {
            return base.AddVertex(null);
        }

        public override Edge AddEdge(object id, Vertex outVertex, Vertex inVertex, string label)
        {
            return base.AddEdge(null, outVertex, inVertex, label);
        }
    }
}
