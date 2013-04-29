namespace Frontenac.Blueprints.Impls.TG
{
    /// <summary>
    /// Implementations are responsible for loading and saving a TinkerGraph data.
    /// </summary>
    interface ITinkerStorage
    {
        TinkerGraph Load(string directory);
        void Save(TinkerGraph graph, string directory);
    }
}
