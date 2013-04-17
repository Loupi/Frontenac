using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Frontenac.Blueprints.Util.Wrappers.Batch.Cache
{
    public interface VertexCache
    {
        object GetEntry(object externalId);

        void Set(Vertex vertex, object externalId);

        void SetId(object vertexId, object externalId);

        bool Contains(object externalId);

        void NewTransaction();
    }
}
