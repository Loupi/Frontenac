using System;
using System.Collections.Generic;
using System.Linq;

namespace Frontenac.Gremlinq
{
    public static partial class GremlinqHelpers
    {
        public static IEnumerable<IVertex<TInModel>> Loop<TModel, TInModel>(
            this IVertex<TModel> element,
            Func<TModel, TInModel> startPointSelector,
            Func<IVertex<TInModel>, IEnumerable<IVertex<TInModel>>> loopFunction,
            int nbIterations)
        {
            if (element == null)
                throw new ArgumentNullException(nameof(element));

            if (startPointSelector == null)
                throw new ArgumentNullException(nameof(startPointSelector));
            if (loopFunction == null)
                throw new ArgumentNullException(nameof(loopFunction));
            if (nbIterations <= 0)
                throw new ArgumentException("nbIterations must be greater than zero");

            var next = (IEnumerable<IVertex<TInModel>>)new[] { element };

            for (var i = 0; i < nbIterations; i++)
                next = next.SelectMany(loopFunction);

            return next;
        }

        public static IEnumerable<IVertex<TInModel>> Loop<TInModel>(
            this IVertex<TInModel> element,
            Func<IVertex<TInModel>, IVertex<TInModel>> loopFunction,
            int nbIterations)
        {
            if (element == null)
                throw new ArgumentNullException(nameof(element));

            if (loopFunction == null)
                throw new ArgumentNullException(nameof(loopFunction));
            if (nbIterations <= 0)
                throw new ArgumentException("nbIterations must be greater than zero");

            var next = (IEnumerable<IVertex<TInModel>>)new[] { element };

            for (var i = 0; i < nbIterations; i++)
                next = next.Select(loopFunction);

            return next;
        }

        public static IEnumerable<IVertex<TInModel>> Loop<TInModel>(
            this IEnumerable<IVertex<TInModel>> elements,
            Func<IVertex<TInModel>, IVertex<TInModel>> loopFunction,
            int nbIterations)
        {
            if (elements == null)
                throw new ArgumentNullException(nameof(elements));
            if (loopFunction == null)
                throw new ArgumentNullException(nameof(loopFunction));
            if (nbIterations <= 0)
                throw new ArgumentException("nbIterations must be greater than zero");

            return elements.SelectMany(t => t.Loop(loopFunction, nbIterations));
        }
    }
}
