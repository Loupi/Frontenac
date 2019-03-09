using System;

namespace Frontenac.Blueprints.Impls.TG
{
    public static class TinkerStorageContract
    {
        public static void ValidateLoad(string directory)
        {
            if (string.IsNullOrWhiteSpace(directory))
                throw new ArgumentNullException(nameof(directory));
        }

        public static void ValidateSave(TinkerGraph tinkerGraph, string directory)
        {
            if (tinkerGraph == null)
                throw new ArgumentNullException(nameof(tinkerGraph));
            if (string.IsNullOrWhiteSpace(directory))
                throw new ArgumentNullException(nameof(directory));
        }
    }
}