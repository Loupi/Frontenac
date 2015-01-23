using System.Diagnostics.Contracts;
using Frontenac.Gremlinq.Contracts;

namespace Frontenac.Gremlinq
{
    [ContractClass(typeof(GremlinqContextFactoryContract))]
    public interface IGremlinqContextFactory
    {
        GremlinqContext Create();
    }
}