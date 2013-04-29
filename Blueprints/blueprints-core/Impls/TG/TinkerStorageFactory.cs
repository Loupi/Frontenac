using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using Frontenac.Blueprints.Util.IO.GML;
using Frontenac.Blueprints.Util.IO.GraphML;
using Frontenac.Blueprints.Util.IO.GraphSON;

namespace Frontenac.Blueprints.Impls.TG
{
    /// <summary>
    /// Constructs TinkerFile instances to load and save TinkerGraph instances.
    /// </summary>
    class TinkerStorageFactory
    {
        static TinkerStorageFactory _factory;

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
                case TinkerGraph.FileType.Java:
                    return new JavaTinkerStorage();
            }

            throw new Exception(string.Format("File Type {0} is not configurable by the factory", fileType));
        }

        /// <summary>
        /// Base class for loading and saving a TinkerGraph.
        /// </summary>
        abstract class AbstractTinkerStorage : ITinkerStorage
        {
            /// <summary>
            /// Clean up the directory that houses the TinkerGraph.
            /// </summary>
            /// <param name="path"></param>
            protected void DeleteFile(string path)
            {
                if (File.Exists(path))
                    File.Delete(path);
            }

            public abstract TinkerGraph Load(string directory);
            public abstract void Save(TinkerGraph graph, string directory);
        }

        /// <summary>
        /// Base class for loading and saving a TinkerGraph where the implementation separates the data from the
        /// meta data stored in the TinkerGraph.
        /// </summary>
        abstract class AbstractSeparateTinkerStorage : AbstractTinkerStorage
        {
            private const string GraphFileMetadata = "/tinkergraph-metadata.dat";

            /// <summary>
            /// Save the data of the graph with the specific file format of the implementation.
            /// </summary>
            public abstract void SaveGraphData(TinkerGraph graph, string directory);

            /// <summary>
            /// Load the data from the graph with the specific file format of the implementation.
            /// </summary>
            public abstract void LoadGraphData(TinkerGraph graph, string directory);

            public override TinkerGraph Load(string directory)
            {
                if (!Directory.Exists(directory))
                    throw new Exception(string.Concat("Directory ", directory, " does not exist"));

                var graph = new TinkerGraph();
                LoadGraphData(graph, directory);

                string filePath = string.Concat(directory, GraphFileMetadata);
                if (File.Exists(filePath))
                    TinkerMetadataReader.Load(graph, filePath);

                return graph;
            }

            public override void Save(TinkerGraph graph, string directory)
            {
                if (!Directory.Exists(directory))
                    Directory.CreateDirectory(directory);

                SaveGraphData(graph, directory);
                string filePath = string.Concat(directory, GraphFileMetadata);
                DeleteFile(filePath);
                TinkerMetadataWriter.Save(graph, filePath);
            }
        }

        /// <summary>
        /// Reads and writes a TinkerGraph to GML as the format for the data.
        /// </summary>
        class GmlTinkerStorage : AbstractSeparateTinkerStorage
        {
            const string GraphFileGml = "/tinkergraph.gml";

            public override void LoadGraphData(TinkerGraph graph, string directory)
            {
                GmlReader.InputGraph(graph, string.Concat(directory, GraphFileGml));
            }

            public override void SaveGraphData(TinkerGraph graph, string directory)
            {
                string filePath = string.Concat(directory, GraphFileGml);
                DeleteFile(filePath);
                GmlWriter.OutputGraph(graph, filePath);
            }
        }

        /// <summary>
        /// Reads and writes a TinkerGraph to GraphSON as the format for the data.
        /// </summary>
        class GraphSonTinkerStorage : AbstractSeparateTinkerStorage
        {
            const string GraphFileGraphson = "/tinkergraph.json";

            public override void LoadGraphData(TinkerGraph graph, string directory)
            {
                GraphSonReader.InputGraph(graph, string.Concat(directory, GraphFileGraphson));
            }

            public override void SaveGraphData(TinkerGraph graph, string directory)
            {
                string filePath = string.Concat(directory, GraphFileGraphson);
                DeleteFile(filePath);
                GraphSonWriter.OutputGraph(graph, filePath, GraphSonMode.EXTENDED);
            }
        }

        /// <summary>
        /// Reads and writes a TinkerGraph to GraphML as the format for the data.
        /// </summary>
        class GraphMlTinkerStorage : AbstractSeparateTinkerStorage
        {
            const string GraphFileGraphml = "/tinkergraph.xml";

            public override void LoadGraphData(TinkerGraph graph, string directory)
            {
                GraphMlReader.InputGraph(graph, string.Concat(directory, GraphFileGraphml));
            }

            public override void SaveGraphData(TinkerGraph graph, string directory)
            {
                string filePath = string.Concat(directory, GraphFileGraphml);
                DeleteFile(filePath);
                GraphMlWriter.OutputGraph(graph, filePath);
            }
        }

        /// <summary>
        /// Reads and writes a TinkerGraph using java object serialization.
        /// </summary>
        class JavaTinkerStorage : AbstractTinkerStorage
        {
            const string GraphFileJava = "/tinkergraph.dat";

            public override TinkerGraph Load(string directory)
            {
                using (var stream = File.OpenRead(string.Concat(directory, GraphFileJava)))
                {
                    var formatter = new BinaryFormatter();
                    return (TinkerGraph)formatter.Deserialize(stream);
                }
            }

            public override void Save(TinkerGraph graph, string directory)
            {
                string filePath = string.Concat(directory, GraphFileJava);
                DeleteFile(filePath);
                using (var stream = File.Create(string.Concat(directory, GraphFileJava)))
                {
                    var formatter = new BinaryFormatter();
                    formatter.Serialize(stream, graph);
                }
            }
        }
    }
}
