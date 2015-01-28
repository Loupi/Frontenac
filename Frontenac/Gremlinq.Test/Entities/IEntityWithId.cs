namespace Frontenac.Gremlinq.Test.Entities
{
    public interface IEntityWithId
    {
        //Id is a special property. It maps directly to the underlying vertex/edge id.
        //You can adjust the type to the one used by your InnerGraph database implementation.
        int Id { get; }
    }
}