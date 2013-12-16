using Frontenac.Blueprints;

namespace Frontenac.Gremlinq
{
    public class QueryWrapper<TModel> : IQueryWrapper<TModel>
    {
        internal QueryWrapper(IQuery query)
        {
            InnerQuery = query;
        }

        public IQuery InnerQuery { get; set; }
    }
}