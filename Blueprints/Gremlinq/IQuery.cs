using Frontenac.Blueprints;

namespace Frontenac.Gremlinq
{
// ReSharper disable UnusedTypeParameter
    public interface IQuery<out TModel>
// ReSharper restore UnusedTypeParameter
    {
        IQuery InnerQuery { get; set; }
    }
}