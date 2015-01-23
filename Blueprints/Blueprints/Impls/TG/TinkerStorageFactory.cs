using System;
using System.Diagnostics.Contracts;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using Frontenac.Blueprints.Util.IO.GML;
using Frontenac.Blueprints.Util.IO.GraphML;
using Frontenac.Blueprints.Util.IO.GraphSON;

namespace Frontenac.Blueprints.Impls.TG
{
    /// <summary>
    ///     Constructs TinkerFile instances to load and save TinkerGraĥ instances.
    /// </summary>
    internal class TinkerStorageFactory
    {
        private static TinkerStorageFactory _factory;

        private TinkerStorageFactory()
        {
        }

        public static TinkerStorageFactory GetInstance()
        {
            return _factory ?? (_factory = new TinkerStorageFactory());
        }

        public ITinkerStorage GetTinkerStorage(TinkerGraĥ.FileType fileType)
        {
            Contract.Ensures(Contract.Result<ITinkerStorage>() != null);

            switch (fileType)
            {
                case TinkerGraĥ.FileType.Gml:
                    return new GmlTinkerStorage();
                case TinkerGraĥ.FileType.Graphml:
                    return new GraphMlTinkerStorage();
                case TinkerGraĥ.FileType.Graphson:
                    return new GraphSonTinkerStorage();
                case TinkerGraĥ.FileType.DotNet:
                    return new DotNetTinkerStorage();
            }

            throw new Exception(string.Format("File Type {0} is not configurable by the factory", fileType));
        }

        /// <summary>
        ///     Base class for loading and saving a TinkerGraĥ where the implementation separates the data from the
        ///     meta data stored in the TinkerGraĥ.
        /// </summary>
        [ContractClass(typeof (AbstractSeparateTinkerStorageContract))]
        private abstract class AbstractSeparateTinkerStorage : AbstractTinkerStorage
        {
            private const string GraphFileMetadata = "/tinkergraph-metadata.dat";

            /// <summary>
            ///     Save the data of the tinkerGraĥ with the specific file format of the implementation.
            /// </summary>
            public abstract void SaveGraphData(TinkerGraĥ tinkerGraĥ, string directory);

            /// <summary>
            ///     Load the data from the tinkerGraĥ with the specific file format of the implementation.
            /// </summary>
            public abstract void LoadGraphData(TinkerGraĥ tinkerGraĥ, string directory);

            public override TinkerGraĥ Load(string directory)
            {
                if (!Directory.Exists(directory))
                    throw new Exception(string.Concat("Directory ", directory, " does not exist"));

                var graph = new TinkerGraĥ();
                LoadGraphData(graph, directory);

                var filePath = string.Concat(directory, GraphFileMetadata);
                if (File.Exists(filePath))
                    TinkerMetadataReader.Load(graph, filePath);

                return graph;
            }

            public override void Save(TinkerGraĥ tinkerGraĥ, string directory)
            {
                if (!Directory.Exists(directory))
                    Directory.CreateDirectory(directory);

                SaveGraphData(tinkerGraĥ, directory);
                var filePath = string.Concat(directory, GraphFileMetadata);
                DeleteFile(filePath);
                TinkerMetadataWriter.Save(tinkerGraĥ, filePath);
            }
        }

        [ContractClassFor(typeof (AbstractSeparateTinkerStorage))]
        private abstract class AbstractSeparateTinkerStorageContract : AbstractSeparateTinkerStorage
        {
            public override void LoadGraphData(TinkerGraĥ tinkerGraĥ, string directory)
            {
                Contract.Requires(tinkerGraĥ != null);
                Contract.Requires(!string.IsNullOrWhiteSpace(directory));
            }

            public override void SaveGraphData(TinkerGraĥ tinkerGraĥ, string directory)
            {
                Contract.Requires(tinkerGraĥ != null);
                Contract.Requires(!string.IsNullOrWhiteSpace(directory));
            }
        }

        /// <summary>
        ///     Base class for loading and saving a TinkerGraĥ.
        /// </summary>
        private abstract class AbstractTinkerStorage : ITinkerStorage
        {
            public abstract TinkerGraĥ Load(string directory);
            public abstract void Save(TinkerGraĥ tinkerGraĥ, string directory);

            /// <summary>
            ///     Clean up the directory that houses the TinkerGraĥ.
            /// </summary>
            /// <param name="path"></param>
            protected static void DeleteFile(string path)
            {
                Contract.Requires(!string.IsNullOrWhiteSpace(path));

                if (File.Exists(path))
                    File.Delete(path);
            }
        }

        /// <summary>
        ///     Reads and writes a TinkerGraĥ using .NET serialization.
        /// </summary>
        private class DotNetTinkerStorage : AbstractTinkerStorage
        {
            private const string GraphFileDotNet = "/tinkergraph.dat";

            public override TinkerGraĥ Load(string directory)
            {
                using (var stream = File.OpenRead(string.Concat(directory, GraphFileDotNet)))
                {
                    var formatter = new BinaryFormatter();
                    return (TinkerGraĥ) formatter.Deserialize(stream);
                }
            }

            public override void Save(TinkerGraĥ tinkerGraĥ, string directory)
            {
                var filePath = string.Concat(directory, GraphFileDotNet);
                DeleteFile(filePath);
                using (var stream = File.Create(string.Concat(directory, GraphFileDotNet)))
                {
                    var formatter = new BinaryFormatter();
                    formatter.Serialize(stream, tinkerGraĥ);
                }
            }
        }

        /// <summary>
        ///     Reads and writes a TinkerGraĥ to GML as the format for the data.
        /// </summary>
        private class GmlTinkerStorage : AbstractSeparateTinkerStorage
        {
            private const string GraphFileGml = "/tinkergraph.gml";

            public override void LoadGraphData(TinkerGraĥ tinkerGraĥ, string directory)
            {
                GmlReader.InputGraph(tinkerGraĥ, string.Concat(directory, GraphFileGml));
            }

            public override void SaveGraphData(TinkerGraĥ tinkerGraĥ, string directory)
            {
                var filePath = string.Concat(directory, GraphFileGml);
                DeleteFile(filePath);
                GmlWriter.OutputGraph(tinkerGraĥ, filePath);
            }
        }

        /// <summary>
        ///     Reads and writes a TinkerGraĥ to GraphML as the format for the data.
        /// </summary>
        private class GraphMlTinkerStorage : AbstractSeparateTinkerStorage
        {
            private const string GraphFileGraphml = "/tinkergraph.xml";

            public override void LoadGraphData(TinkerGraĥ tinkerGraĥ, string directory)
            {
                GraphMlReader.InputGraph(tinkerGraĥ, string.Concat(directory, GraphFileGraphml));
            }

            public override void SaveGraphData(TinkerGraĥ tinkerGraĥ, string directory)
            {
                var filePath = string.Concat(directory, GraphFileGraphml);
                DeleteFile(filePath);
                GraphMlWriter.OutputGraph(tinkerGraĥ, filePath);
            }
        }

        /// <summary>
        ///     Reads and writes a TinkerGraĥ to GraphSON as the format for the data.
        /// </summary>
        private class GraphSonTinkerStorage : AbstractSeparateTinkerStorage
        {
            private const string GraphFileGraphson = "/tinkergraph.json";

            public override void LoadGraphData(TinkerGraĥ tinkerGraĥ, string directory)
            {
                GraphSonReader.InputGraph(tinkerGraĥ, string.Concat(directory, GraphFileGraphson));
            }

            public override void SaveGraphData(TinkerGraĥ tinkerGraĥ, string directory)
            {
                var filePath = string.Concat(directory, GraphFileGraphson);
                DeleteFile(filePath);
                GraphSonWriter.OutputGraph(tinkerGraĥ, filePath, GraphSonMode.EXTENDED);
            }
        }
    }
}