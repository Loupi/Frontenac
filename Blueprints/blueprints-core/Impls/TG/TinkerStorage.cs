﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Frontenac.Blueprints.Impls.TG
{
    /// <summary>
    /// Implementations are responsible for loading and saving a TinkerGraph data.
    /// </summary>
    interface TinkerStorage
    {
        TinkerGraph Load(string directory);
        void Save(TinkerGraph graph, string directory);
    }
}