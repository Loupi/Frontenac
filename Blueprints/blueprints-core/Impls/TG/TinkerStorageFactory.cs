using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;
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
        static TinkerStorageFactory factory;

        private TinkerStorageFactory()
        {
        }

        public static TinkerStorageFactory GetInstance()
        {
            if (factory == null)
                factory = new TinkerStorageFactory();

            return factory;
        }

        public TinkerStorage GetTinkerStorage(TinkerGraph.FileType fileType)
        {
            switch (fileType)
            {
                case TinkerGraph.FileType.GML:
                    return new GMLTinkerStorage();
                case TinkerGraph.FileType.GRAPHML:
                    return new GraphMLTinkerStorage();
                case TinkerGraph.FileType.GRAPHSON:
                    return new GraphSONTinkerStorage();
                case TinkerGraph.FileType.JAVA:
                    return new JavaTinkerStorage();
            }

            throw new Exception(string.Format("File Type {0} is not configurable by the factory", fileType));
        }

        /// <summary>
        /// Base class for loading and saving a TinkerGraph.
        /// </summary>
        abstract class AbstractTinkerStorage : TinkerStorage
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
            protected const string GRAPH_FILE_METADATA = "/tinkergraph-metadata.dat";

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

                TinkerGraph graph = new TinkerGraph();
                LoadGraphData(graph, directory);

                string filePath = string.Concat(directory, GRAPH_FILE_METADATA);
                if (File.Exists(filePath))
                    TinkerMetadataReader.Load(graph, filePath);

                return graph;
            }

            public override void Save(TinkerGraph graph, string directory)
            {
                if (!Directory.Exists(directory))
                    Directory.CreateDirectory(directory);

                SaveGraphData(graph, directory);
                string filePath = string.Concat(directory, GRAPH_FILE_METADATA);
                DeleteFile(filePath);
                TinkerMetadataWriter.Save(graph, filePath);
            }
        }

        /// <summary>
        /// Reads and writes a TinkerGraph to GML as the format for the data.
        /// </summary>
        class GMLTinkerStorage : AbstractSeparateTinkerStorage
        {
            const string GRAPH_FILE_GML = "/tinkergraph.gml";

            public override void LoadGraphData(TinkerGraph graph, string directory)
            {
                GMLReader.InputGraph(graph, string.Concat(directory + GRAPH_FILE_GML));
            }

            public override void SaveGraphData(TinkerGraph graph, string directory)
            {
                string filePath = string.Concat(directory, GRAPH_FILE_GML);
                DeleteFile(filePath);
                GMLWriter.OutputGraph(graph, filePath);
            }
        }

        /// <summary>
        /// Reads and writes a TinkerGraph to GraphSON as the format for the data.
        /// </summary>
        class GraphSONTinkerStorage : AbstractSeparateTinkerStorage
        {
            const string GRAPH_FILE_GRAPHSON = "/tinkergraph.json";

            public override void LoadGraphData(TinkerGraph graph, string directory)
            {
                GraphSONReader.InputGraph(graph, string.Concat(directory + GRAPH_FILE_GRAPHSON));
            }

            public override void SaveGraphData(TinkerGraph graph, string directory)
            {
                string filePath = string.Concat(directory, GRAPH_FILE_GRAPHSON);
                DeleteFile(filePath);
                GraphSONWriter.OutputGraph(graph, filePath, GraphSONMode.EXTENDED);
            }
        }

        /// <summary>
        /// Reads and writes a TinkerGraph to GraphML as the format for the data.
        /// </summary>
        class GraphMLTinkerStorage : AbstractSeparateTinkerStorage
        {
            const string GRAPH_FILE_GRAPHML = "/tinkergraph.xml";

            public override void LoadGraphData(TinkerGraph graph, string directory)
            {
                GraphMLReader.InputGraph(graph, string.Concat(directory, GRAPH_FILE_GRAPHML));
            }

            public override void SaveGraphData(TinkerGraph graph, string directory)
            {
                string filePath = string.Concat(directory, GRAPH_FILE_GRAPHML);
                DeleteFile(filePath);
                GraphMLWriter.OutputGraph(graph, filePath);
            }
        }

        /// <summary>
        /// Reads and writes a TinkerGraph using java object serialization.
        /// </summary>
        class JavaTinkerStorage : AbstractTinkerStorage
        {
            const string GRAPH_FILE_JAVA = "/tinkergraph.dat";

            public override TinkerGraph Load(string directory)
            {
                using (var stream = File.OpenRead(string.Concat(directory, GRAPH_FILE_JAVA)))
                {
                    BinaryFormatter formatter = new BinaryFormatter();
                    return (TinkerGraph)formatter.Deserialize(stream);
                }
            }

            public override void Save(TinkerGraph graph, string directory)
            {
                string filePath = string.Concat(directory, GRAPH_FILE_JAVA);
                DeleteFile(filePath);
                using (var stream = File.Create(string.Concat(directory, GRAPH_FILE_JAVA)))
                {
                    BinaryFormatter formatter = new BinaryFormatter();
                    formatter.Serialize(stream, graph);
                }
            }
        }
    }
}
