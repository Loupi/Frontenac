namespace Frontenac.Blueprints.Impls
{
    public abstract class GraphTest : BaseTest
    {
        public abstract IGraph GenerateGraph();

        public abstract IGraph GenerateGraph(string graphDirectoryName);
    }
}