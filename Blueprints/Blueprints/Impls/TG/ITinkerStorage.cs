using System.Diagnostics.Contracts;

namespace Frontenac.Blueprints.Impls.TG
{
    /// <summary>
    ///     Implementations are responsible for loading and saving a TinkerGraĥ data.
    /// </summary>
    [ContractClass(typeof (TinkerStorageContract))]
    internal interface ITinkerStorage
    {
        TinkerGraĥ Load(string directory);
        void Save(TinkerGraĥ tinkerGraĥ, string directory);
    }
}