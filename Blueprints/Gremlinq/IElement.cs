using Frontenac.Blueprints;

namespace Frontenac.Gremlinq
{
    public interface IElement<out TModel> : IElement
    {
        IElement Element { get; }
        TModel Model { get; }
    }
}