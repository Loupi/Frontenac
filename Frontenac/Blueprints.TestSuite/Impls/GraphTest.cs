namespace Frontenac.Blueprints.Impls
{
    public abstract class GraphTest : BaseTest
    {
        public abstract IGraph GenerateGraph();

        public abstract IGraph GenerateGraph(string graphDirectoryName);

        public abstract ITransactionalGraph GenerateTransactionalGraph();

        public abstract ITransactionalGraph GenerateTransactionalGraph(string graphDirectoryName);
    }
}