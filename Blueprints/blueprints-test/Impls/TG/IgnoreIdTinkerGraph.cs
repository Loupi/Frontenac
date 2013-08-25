using System;

namespace Frontenac.Blueprints.Impls.TG
{
    /// <summary>
    /// This is class is an in-memory variant of TinkerGraph that ignores the supplied ids
    /// and instead uses its own internal id scheme.
    /// This is meant to be used for testing only.
    /// </summary>
    [Serializable]
    public class IgnoreIdTinkerGraph : TinkerGraph
    {
        public IgnoreIdTinkerGraph()
        {

        }

        public IgnoreIdTinkerGraph(string directory)
            : base(directory)
        {

        }

        public override Features Features
        {
            get
            {
                Features f = base.Features.CopyFeatures();
                f.IgnoresSuppliedIds = true;
                return f;
            }
        }

        public override IVertex AddVertex(object id)
        {
            return base.AddVertex(null);
        }

        public override IEdge AddEdge(object id, IVertex outVertex, IVertex inVertex, string label)
        {
            return base.AddEdge(null, outVertex, inVertex, label);
        }
    }
}
