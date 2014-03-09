using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
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
            Contract.Requires(element != null);
            Contract.Requires(startPointSelector != null);
            Contract.Requires(loopFunction != null);
            Contract.Requires(nbIterations > 0);
            Contract.Ensures(Contract.Result<IEnumerable<IVertex<TInModel>>>() != null);

            var next = (IEnumerable<IVertex<TInModel>>)new[] { element };

            for (var i = 0; i < nbIterations; i++)
                next = next.SelectMany(loopFunction);

            return next;
        }

        public static IEnumerable<IVertex<TInModel>> Loop<TInModel>(
            this IVertex<TInModel> element,
            Func<IVertex<TInModel>, IEnumerable<IVertex<TInModel>>> loopFunction,
            int nbIterations)
        {
            Contract.Requires(element != null);
            Contract.Requires(loopFunction != null);
            Contract.Requires(nbIterations > 0);
            Contract.Ensures(Contract.Result<IEnumerable<IVertex<TInModel>>>() != null);

            var next = (IEnumerable<IVertex<TInModel>>)new[] { element };

            for (var i = 0; i < nbIterations; i++)
                next = next.SelectMany(loopFunction);

            return next;
        }

        public static IEnumerable<IVertex<TInModel>> Loop<TInModel>(
            this IEnumerable<IVertex<TInModel>> elements,
            Func<IVertex<TInModel>, IEnumerable<IVertex<TInModel>>> loopFunction,
            int nbIterations)
        {
            Contract.Requires(elements != null);
            Contract.Requires(loopFunction != null);
            Contract.Requires(nbIterations > 0);
            Contract.Ensures(Contract.Result<IEnumerable<IVertex<TInModel>>>() != null);

            return elements.SelectMany(t => t.Loop(loopFunction, nbIterations));
        }
    }
}
