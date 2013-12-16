using Frontenac.Grave.Esent.Serializers;
using Microsoft.Isam.Esent.Interop;

namespace Frontenac.Grave.Esent
{
    public class EsentConfigTable : EsentVertexTable
    {
        public EsentConfigTable(Session session, IContentSerializer contentSerializer)
            : base(session, contentSerializer)
        {
            TableName = "Config";
        }
    }
}