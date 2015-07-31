namespace Frontenac.Gremlinq.Test.Entities
{
    public interface IChildren : IEntityWithId
    {
        bool IsAdopted { get; set; }
    }
}