using System.Diagnostics.Contracts;

namespace Frontenac.Blueprints.Impls.TG
{
    /// <summary>
    ///     Implementations are responsible for loading and saving a TinkerGraph data.
    /// </summary>
    [ContractClass(typeof (TinkerStorageContract))]
    internal interface ITinkerStorage
    {
        TinkerGraph Load(string directory);
        void Save(TinkerGraph graph, string directory);
    }
}