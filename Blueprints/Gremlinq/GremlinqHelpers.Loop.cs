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
    }
}
