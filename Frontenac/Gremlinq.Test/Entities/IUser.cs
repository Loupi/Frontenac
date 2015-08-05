using System.Collections.Generic;

namespace Frontenac.Gremlinq.Test.Entities
{
    public interface IUser : IEntityWithId
    {
        Gender Gender { get; set; }
        ICollection<KeyValuePair<IRating, IUser>> Rated { get; set; }
        IRating Rating { get; set; }
    }
}