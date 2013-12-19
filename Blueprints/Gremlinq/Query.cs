using Frontenac.Blueprints;

namespace Frontenac.Gremlinq
{
    public class Query<TModel> : IQuery<TModel>
    {
        internal Query(IQuery query)
        {
            InnerQuery = query;
        }

        public IQuery InnerQuery { get; set; }
    }
}