namespace Frontenac.Gremlinq
{
    public interface IElementWrapper<out TElement, out TModel>
    {
        TElement Element { get; }
        TModel Model { get; }
    }
}