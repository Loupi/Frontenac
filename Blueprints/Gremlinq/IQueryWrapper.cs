using Frontenac.Blueprints;

namespace Frontenac.Gremlinq
{
    public interface IQueryWrapper<out TModel>
    {
        IQuery InnerQuery { get; set; }
    }
}