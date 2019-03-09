using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using Frontenac.Blueprints.Util.IO.GML;
using Frontenac.Blueprints.Util.IO.GraphML;
using Frontenac.Blueprints.Util.IO.GraphSON;

namespace Frontenac.Blueprints.Impls.TG
{
    /// <summary>
    ///     Constructs TinkerFile instances to load and save TinkerGraph instances.
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

        public ITinkerStorage GetTinkerStorage(TinkerGraph.FileType fileType)
        {
            switch (fileType)
            {
                case TinkerGraph.FileType.Gml:
                    return new GmlTinkerStorage();
                case TinkerGraph.FileType.Graphml:
                    return new GraphMlTinkerStorage();
                case TinkerGraph.FileType.Graphson:
                    return new GraphSonTinkerStorage();
                case TinkerGraph.FileType.DotNet:
                    return new DotNetTinkerStorage();
            }

            throw new Exception($"File Type {fileType} is not configurable by the factory");
        }

        /// <summary>
        ///     Base class for loading and saving a TinkerGraph where the implementation separates the data from the
        ///     meta data stored in the TinkerGraph.
        /// </summary>
        private abstract class AbstractSeparateTinkerStorage : AbstractTinkerStorage
        {
            private const string GraphFileMetadata = "/tinkergraph-metadata.dat";

            /// <summary>
            ///     Save the data of the TinkerGraph with the specific file format of the implementation.
            /// </summary>
            public abstract void SaveGraphData(TinkerGraph tinkerGraph, string directory);

            /// <summary>
            ///     Load the data from the TinkerGraph with the specific file format of the implementation.
            /// </summary>
            public abstract void LoadGraphData(TinkerGraph tinkerGraph, string directory);

            public override TinkerGraph Load(string directory)
            {
                TinkerStorageContract.ValidateLoad(directory);

                if (!Directory.Exists(directory))
                    throw new Exception(string.Concat("Directory ", directory, " does not exist"));

                var graph = new TinkerGraph();
                LoadGraphData(graph, directory);

                var filePath = string.Concat(directory, GraphFileMetadata);
                if (File.Exists(filePath))
                    TinkerMetadataReader.Load(graph, filePath);

                return graph;
            }

            public override void Save(TinkerGraph tinkerGraph, string directory)
            {
                TinkerStorageContract.ValidateSave(tinkerGraph, directory);

                if (!Directory.Exists(directory))
                    Directory.CreateDirectory(directory);

                SaveGraphData(tinkerGraph, directory);
                var filePath = string.Concat(directory, GraphFileMetadata);
                DeleteFile(filePath);
                TinkerMetadataWriter.Save(tinkerGraph, filePath);
            }
        }
        private static class AbstractSeparateTinkerStorageContract
        {
            public static void ValidateLoadGraphData(TinkerGraph tinkerGraph, string directory)
            {
                if (tinkerGraph == null)
                    throw new ArgumentNullException(nameof(tinkerGraph));
                if (string.IsNullOrWhiteSpace(directory))
                    throw new ArgumentNullException(nameof(directory));
            }

            public static void ValidateSaveGraphData(TinkerGraph tinkerGraph, string directory)
            {
                if (tinkerGraph == null)
                    throw new ArgumentNullException(nameof(tinkerGraph));
                if (string.IsNullOrWhiteSpace(directory))
                    throw new ArgumentNullException(nameof(directory));
            }
        }

        /// <summary>
        ///     Base class for loading and saving a TinkerGraph.
        /// </summary>
        private abstract class AbstractTinkerStorage : ITinkerStorage
        {
            public abstract TinkerGraph Load(string directory);
            public abstract void Save(TinkerGraph tinkerGraph, string directory);

            /// <summary>
            ///     Clean up the directory that houses the TinkerGraph.
            /// </summary>
            /// <param name="path"></param>
            protected static void DeleteFile(string path)
            {
                if (string.IsNullOrWhiteSpace(path))
                    throw new ArgumentNullException(nameof(path));

                if (File.Exists(path))
                    File.Delete(path);
            }
        }

        /// <summary>
        ///     Reads and writes a TinkerGraph using .NET serialization.
        /// </summary>
        private class DotNetTinkerStorage : AbstractTinkerStorage
        {
            private const string GraphFileDotNet = "/tinkergraph.dat";

            public override TinkerGraph Load(string directory)
            {
                TinkerStorageContract.ValidateLoad(directory);

                using (var stream = File.OpenRead(string.Concat(directory, GraphFileDotNet)))
                {
                    var formatter = new BinaryFormatter();
                    return (TinkerGraph) formatter.Deserialize(stream);
                }
            }

            public override void Save(TinkerGraph tinkerGraph, string directory)
            {
                TinkerStorageContract.ValidateSave(tinkerGraph, directory);

                var filePath = string.Concat(directory, GraphFileDotNet);
                DeleteFile(filePath);
                using (var stream = File.Create(string.Concat(directory, GraphFileDotNet)))
                {
                    var formatter = new BinaryFormatter();
                    formatter.Serialize(stream, tinkerGraph);
                }
            }
        }

        /// <summary>
        ///     Reads and writes a TinkerGraph to GML as the format for the data.
        /// </summary>
        private class GmlTinkerStorage : AbstractSeparateTinkerStorage
        {
            private const string GraphFileGml = "/tinkergraph.gml";

            public override void LoadGraphData(TinkerGraph tinkerGraph, string directory)
            {
                AbstractSeparateTinkerStorageContract.ValidateLoadGraphData(tinkerGraph, directory);

                GmlReader.InputGraph(tinkerGraph, string.Concat(directory, GraphFileGml));
            }

            public override void SaveGraphData(TinkerGraph tinkerGraph, string directory)
            {
                AbstractSeparateTinkerStorageContract.ValidateSaveGraphData(tinkerGraph, directory);

                var filePath = string.Concat(directory, GraphFileGml);
                DeleteFile(filePath);
                GmlWriter.OutputGraph(tinkerGraph, filePath);
            }
        }

        /// <summary>
        ///     Reads and writes a TinkerGraph to GraphML as the format for the data.
        /// </summary>
        private class GraphMlTinkerStorage : AbstractSeparateTinkerStorage
        {
            private const string GraphFileGraphml = "/tinkergraph.xml";

            public override void LoadGraphData(TinkerGraph tinkerGraph, string directory)
            {
                AbstractSeparateTinkerStorageContract.ValidateLoadGraphData(tinkerGraph, directory);

                GraphMlReader.InputGraph(tinkerGraph, string.Concat(directory, GraphFileGraphml));
            }

            public override void SaveGraphData(TinkerGraph tinkerGraph, string directory)
            {
                AbstractSeparateTinkerStorageContract.ValidateSaveGraphData(tinkerGraph, directory);

                var filePath = string.Concat(directory, GraphFileGraphml);
                DeleteFile(filePath);
                GraphMlWriter.OutputGraph(tinkerGraph, filePath);
            }
        }

        /// <summary>
        ///     Reads and writes a TinkerGraph to GraphSON as the format for the data.
        /// </summary>
        private class GraphSonTinkerStorage : AbstractSeparateTinkerStorage
        {
            private const string GraphFileGraphson = "/tinkergraph.json";

            public override void LoadGraphData(TinkerGraph tinkerGraph, string directory)
            {
                AbstractSeparateTinkerStorageContract.ValidateLoadGraphData(tinkerGraph, directory);

                GraphSonReader.InputGraph(tinkerGraph, string.Concat(directory, GraphFileGraphson));
            }

            public override void SaveGraphData(TinkerGraph tinkerGraph, string directory)
            {
                AbstractSeparateTinkerStorageContract.ValidateSaveGraphData(tinkerGraph, directory);

                var filePath = string.Concat(directory, GraphFileGraphson);
                DeleteFile(filePath);
                GraphSonWriter.OutputGraph(tinkerGraph, filePath, GraphSonMode.EXTENDED);
            }
        }
    }
}