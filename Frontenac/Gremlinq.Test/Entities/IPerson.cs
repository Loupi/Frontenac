using System.Collections.Generic;
using Frontenac.Blueprints;

namespace Frontenac.Gremlinq.Test.Entities
{
    public interface IPerson : IEntityWithId
    {
        string Name { get; set; }
        int Age { get; set; }

        //A relation pointing to a father vertex. The edge is not typed, and it's out vertex is of type IPerson.
        //Default direction is Out. Uses the property name "Children" for label.
        IPerson Father { get; set; }

        //A relation pointing to a mother vertex. The edge is of type IChildren, and it's in vertex is of type IPerson.
        KeyValuePair<IChildren, IPerson> Mother { get; set; }

        //A relation pointing to children vertices. The edge is of type IChildren, and it's in vertex is of type IPerson
        //Default direction is Out. Uses the property name "Children" for label.
        ICollection<KeyValuePair<IChildren, IPerson>> Children { get; set; }

        //Relation direction and label is overriden with RelationAttribute.
        [Relation(Direction.In, "Children")]
        IEnumerable<IPerson> Parents { get; set; }

        //You can override the same relation with wrapped models too.
        [Relation(Direction.In, "Children")]
        IEnumerable<IVertex<IPerson>> WrappedParents { get; set; }

        //A relation pointing to job vertices. The edge is not typed, and it's in vertex is of type IJob
        ICollection<IJob> Job { get; set; }
    }
}