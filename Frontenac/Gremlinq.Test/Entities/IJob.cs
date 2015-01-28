using System.Collections.Generic;
using Frontenac.Blueprints;

namespace Frontenac.Gremlinq.Test.Entities
{
    public interface IJob : IEntityWithId
    {
        string Title { get; set; }
        string Domain { get; set; }

        //A relation pointing to the persons with this job.
        //The direction is overriden to In, and the label is overriden with "Job".
        [Relation(Direction.In, "Job")]
        IEnumerable<IPerson> Persons { get; set; }
    }
}