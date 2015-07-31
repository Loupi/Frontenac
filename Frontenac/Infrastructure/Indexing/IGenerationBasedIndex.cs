namespace Frontenac.Infrastructure.Indexing
{
    public interface IGenerationBasedIndex
    {
        void WaitForGeneration();
        void UpdateGeneration(long generation);
    }
}
