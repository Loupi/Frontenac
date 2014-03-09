using Frontenac.Blueprints;

namespace Frontenac.Gremlinq
{
    public interface IVertex<out TModel> : IElement<TModel>, IVertex
    {
        IVertex Vertex { get; }
    }
}