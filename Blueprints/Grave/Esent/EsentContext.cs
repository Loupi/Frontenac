using System.Diagnostics.Contracts;
using Frontenac.Grave.Esent.Serializers;
using Microsoft.Isam.Esent.Interop;

namespace Frontenac.Grave.Esent
{
    public class EsentContext : EsentContextBase
    {
        public EsentContext(Session session, string databaseName, IContentSerializer contentSerializer) :
            base(session, databaseName, contentSerializer)
        {
            Contract.Requires(session != null);
            Contract.Requires(contentSerializer != null);

            OpenDatabase();
        }
    }
}