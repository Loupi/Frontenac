namespace Frontenac.Blueprints.Impls.TG
{
    /// <summary>
    ///     Implementations are responsible for loading and saving a TinkerGraph data.
    /// </summary>
    internal interface ITinkerStorage
    {
        TinkerGraph Load(string directory);
        void Save(TinkerGraph tinkerGraph, string directory);
    }
}