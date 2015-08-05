using System.Collections.Generic;

namespace Frontenac.Gremlinq.Test.Entities
{
    public interface IRating : IEntityWithId
    {
        int Rating { get; set; }
        ICollection<IUser> RatingOf { get; set; }
    }
}