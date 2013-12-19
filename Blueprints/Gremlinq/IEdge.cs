using Frontenac.Blueprints;

namespace Frontenac.Gremlinq
{
    public interface IEdge<out TModel> : IElement<TModel>, IEdge
    {
        IEdge Edge { get; }
    }
}