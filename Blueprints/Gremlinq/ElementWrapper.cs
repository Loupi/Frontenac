using Frontenac.Blueprints;

namespace Frontenac.Gremlinq
{
    public class ElementWrapper<TElement, TModel> : IElementWrapper<TElement, TModel> where TElement : IElement
    {
        internal ElementWrapper(TElement element, TModel model)
        {
            Element = element;
            Model = model;
        }

        public TElement Element { get; private set; }
        public TModel Model { get; private set; }
    }
}