using System.Diagnostics.Contracts;
using Frontenac.Infrastructure.Serializers;
using Microsoft.Isam.Esent.Interop;

namespace Frontenac.Grave.Esent
{
    public class EsentConfigTable : EsentVertexTable
    {
        public EsentConfigTable(Session session, IContentSerializer contentSerializer)
            : base(session, contentSerializer)
        {
            Contract.Requires(session != null);
            Contract.Requires(contentSerializer != null);

            TableName = "Config";
        }
    }
}