using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using Frontenac.Blueprints.Util.IO.GML;
using Frontenac.Blueprints.Util.IO.GraphML;
using Frontenac.Blueprints.Util.IO.GraphSON;
using VelocityGraph;
using VelocityDb.Session;

namespace Frontenac.Blueprints.Impls.VG
{
    /// <summary>
    /// Constructs TinkerFile instances to load and save TinkerGraph instances.
    /// </summary>
    public class VelocityGraphStorageFactory
    {
        static VelocityGraphStorageFactory _factory;

        private VelocityGraphStorageFactory()
        {
        }

        public static VelocityGraphStorageFactory GetInstance()
        {
            return _factory ?? (_factory = new VelocityGraphStorageFactory());
        }

        public IVGStorage GetVGStorage(FileType fileType)
        {
            switch (fileType)
            {
                case FileType.Gml:
                    return new GmlVGStorage();
                case FileType.Graphml:
                    return new GraphMlVGStorage();
                case FileType.Graphson:
                    return new GraphSonVGStorage();
                case FileType.Java:
                    return new JavaVGStorage();
            }

            throw new Exception(string.Format("File Type {0} is not configurable by the factory", fileType));
        }

        /// <summary>
        /// Base class for loading and saving a TinkerGraph.
        /// </summary>
        abstract class AbstractVGStorage : IVGStorage
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

            public abstract Graph Load(string directory);
            public abstract void Save(Graph graph, string directory);
        }

        /// <summary>
        /// Base class for loading and saving a TinkerGraph where the implementation separates the data from the
        /// meta data stored in the TinkerGraph.
        /// </summary>
        abstract class AbstractSeparateVGStorage : AbstractVGStorage
        {
            private const string GraphFileMetadata = "/tinkergraph-metadata.dat";

            /// <summary>
            /// Save the data of the graph with the specific file format of the implementation.
            /// </summary>
            public abstract void SaveGraphData(Graph graph, string directory);

            /// <summary>
            /// Load the data from the graph with the specific file format of the implementation.
            /// </summary>
            public abstract void LoadGraphData(Graph graph, string directory);

            public override Graph Load(string directory)
            {
                if (!Directory.Exists(directory))
                    throw new Exception(string.Concat("Directory ", directory, " does not exist"));
                SessionBase session = new SessionNoServer(directory);
                var graph = new Graph(session);
                LoadGraphData(graph, directory);
                string filePath = string.Concat(directory, GraphFileMetadata);
                return graph;
            }

            public override void Save(Graph graph, string directory)
            {
                if (!Directory.Exists(directory))
                    Directory.CreateDirectory(directory);

                SaveGraphData(graph, directory);
            }
        }

        /// <summary>
        /// Reads and writes a TinkerGraph to GML as the format for the data.
        /// </summary>
        class GmlVGStorage : AbstractSeparateVGStorage
        {
            const string GraphFileGml = "/tinkergraph.gml";

            public override void LoadGraphData(Graph graph, string directory)
            {
                GmlReader.InputGraph(graph, string.Concat(directory, GraphFileGml));
            }

            public override void SaveGraphData(Graph graph, string directory)
            {
                string filePath = string.Concat(directory, GraphFileGml);
                DeleteFile(filePath);
                GmlWriter.OutputGraph(graph, filePath);
            }
        }

        /// <summary>
        /// Reads and writes a TinkerGraph to GraphSON as the format for the data.
        /// </summary>
        class GraphSonVGStorage : AbstractSeparateVGStorage
        {
            const string GraphFileGraphson = "/tinkergraph.json";

            public override void LoadGraphData(Graph graph, string directory)
            {
                GraphSONReader.InputGraph(graph, string.Concat(directory, GraphFileGraphson));
            }

            public override void SaveGraphData(Graph graph, string directory)
            {
                string filePath = string.Concat(directory, GraphFileGraphson);
                DeleteFile(filePath);
                GraphSonWriter.OutputGraph(graph, filePath, GraphSONMode.EXTENDED);
            }
        }

        /// <summary>
        /// Reads and writes a TinkerGraph to GraphML as the format for the data.
        /// </summary>
        class GraphMlVGStorage : AbstractSeparateVGStorage
        {
            const string GraphFileGraphml = "/tinkergraph.xml";

            public override void LoadGraphData(Graph graph, string directory)
            {
                GraphMlReader.InputGraph(graph, string.Concat(directory, GraphFileGraphml));
            }

            public override void SaveGraphData(Graph graph, string directory)
            {
                string filePath = string.Concat(directory, GraphFileGraphml);
                DeleteFile(filePath);
                GraphMlWriter.OutputGraph(graph, filePath);
            }
        }

        /// <summary>
        /// Reads and writes a TinkerGraph using java object serialization.
        /// </summary>
        class JavaVGStorage : AbstractVGStorage
        {
            const string GraphFileJava = "/tinkergraph.dat";

            public override Graph Load(string directory)
            {
                using (var stream = File.OpenRead(string.Concat(directory, GraphFileJava)))
                {
                    var formatter = new BinaryFormatter();
                    return (Graph)formatter.Deserialize(stream);
                }
            }

            public override void Save(Graph graph, string directory)
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
