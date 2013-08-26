using System;
using VelocityDb.Session;
using VelocityGraph;

namespace Frontenac.Blueprints.Impls.VG
{
    /// <summary>
    /// This is class is an in-memory variant of TinkerGraph that ignores the supplied ids
    /// and instead uses its own internal id scheme.
    /// This is meant to be used for testing only.
    /// </summary>
    [Serializable]
    public class IgnoreIdVelocityGraph : Graph
    {
        public IgnoreIdVelocityGraph(SessionBase session):base(session)
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
