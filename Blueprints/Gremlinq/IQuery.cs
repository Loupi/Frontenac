using Frontenac.Blueprints;

namespace Frontenac.Gremlinq
{
    public interface IQuery<out TModel>
    {
        IQuery InnerQuery { get; set; }
    }
}