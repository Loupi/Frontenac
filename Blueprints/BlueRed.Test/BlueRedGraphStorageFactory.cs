using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using Frontenac.Blueprints.Util.IO.GML;
using Frontenac.Blueprints.Util.IO.GraphML;
using Frontenac.Blueprints.Util.IO.GraphSON;

namespace Frontenac.BlueRed.Tests
{
    /// <summary>
    ///     Constructs GraveFile instances to load and save GraveGraph instances.
    /// </summary>
    public class BlueRedGraphStorageFactory
    {
        private static BlueRedGraphStorageFactory _factory;

        private BlueRedGraphStorageFactory()
        {
        }

        public static BlueRedGraphStorageFactory GetInstance()
        {
            return _factory ?? (_factory = new BlueRedGraphStorageFactory());
        }

        public IBlueRedStorage GetBlueRedStorage(FileType fileType)
        {
            switch (fileType)
            {
                case FileType.Gml:
                    return new GmlBlueRedStorage();
                case FileType.Graphml:
                    return new GraphMlBlueRedStorage();
                case FileType.Graphson:
                    return new GraphSonBlueRedStorage();
                case FileType.DotNet:
                    return new DotNetBlueRedStorage();
            }

            throw new Exception(string.Format("File Type {0} is not configurable by the factory", fileType));
        }

        /// <summary>
        ///     Base class for loading and saving a GraveGraph.
        /// </summary>
        private abstract class AbstractBlueRedStorage : IBlueRedStorage
        {
            public abstract RedisGraph Load(string directory);
            public abstract void Save(RedisGraph graph, string directory);

            /// <summary>
            ///     Clean up the directory that houses the RedisGraph.
            /// </summary>
            /// <param name="path"></param>
            protected void DeleteFile(string path)
            {
                if (File.Exists(path))
                    File.Delete(path);
            }
        }

        /// <summary>
        ///     Base class for loading and saving a GraveGraph where the implementation separates the data from the
        ///     meta data stored in the RedisGraph.
        /// </summary>
        private abstract class AbstractSeparateBlueRedStorage : AbstractBlueRedStorage
        {
            /// <summary>
            ///     Save the data of the graph with the specific file format of the implementation.
            /// </summary>
            public abstract void SaveGraphData(RedisGraph graph, string directory);

            /// <summary>
            ///     Load the data from the graph with the specific file format of the implementation.
            /// </summary>
            public abstract void LoadGraphData(RedisGraph graph, string directory);

            public override RedisGraph Load(string directory)
            {
                if (!Directory.Exists(directory))
                    throw new Exception(string.Concat("Directory ", directory, " does not exist"));

                var graph = BlueRedFactory.CreateGraph();
                LoadGraphData(graph, directory);

                return graph;
            }

            public override void Save(RedisGraph graph, string directory)
            {
                if (!Directory.Exists(directory))
                    Directory.CreateDirectory(directory);

                SaveGraphData(graph, directory);
            }
        }

        /// <summary>
        ///     Reads and writes a GraveGraph using .NET object serialization.
        /// </summary>
        private class DotNetBlueRedStorage : AbstractBlueRedStorage
        {
            private const string GraphFileDotNet = "/tinkergraph.dat";

            public override RedisGraph Load(string directory)
            {
                using (var stream = File.OpenRead(string.Concat(directory, GraphFileDotNet)))
                {
                    var formatter = new BinaryFormatter();
                    return (RedisGraph) formatter.Deserialize(stream);
                }
            }

            public override void Save(RedisGraph graph, string directory)
            {
                var filePath = string.Concat(directory, GraphFileDotNet);
                DeleteFile(filePath);
                using (var stream = File.Create(string.Concat(directory, GraphFileDotNet)))
                {
                    var formatter = new BinaryFormatter();
                    formatter.Serialize(stream, graph);
                }
            }
        }

        /// <summary>
        ///     Reads and writes a GraveGraph to GML as the format for the data.
        /// </summary>
        private class GmlBlueRedStorage : AbstractSeparateBlueRedStorage
        {
            private const string GraphFileGml = "/Gravegraph.gml";

            public override void LoadGraphData(RedisGraph graph, string directory)
            {
                GmlReader.InputGraph(graph, string.Concat(directory, GraphFileGml));
            }

            public override void SaveGraphData(RedisGraph graph, string directory)
            {
                var filePath = string.Concat(directory, GraphFileGml);
                DeleteFile(filePath);
                GmlWriter.OutputGraph(graph, filePath);
            }
        }

        /// <summary>
        ///     Reads and writes a GraveGraph to GraphML as the format for the data.
        /// </summary>
        private class GraphMlBlueRedStorage : AbstractSeparateBlueRedStorage
        {
            private const string GraphFileGraphml = "/Gravegraph.xml";

            public override void LoadGraphData(RedisGraph graph, string directory)
            {
                GraphMlReader.InputGraph(graph, string.Concat(directory, GraphFileGraphml));
            }

            public override void SaveGraphData(RedisGraph graph, string directory)
            {
                var filePath = string.Concat(directory, GraphFileGraphml);
                DeleteFile(filePath);
                GraphMlWriter.OutputGraph(graph, filePath);
            }
        }

        /// <summary>
        ///     Reads and writes a GraveGraph to GraphSON as the format for the data.
        /// </summary>
        private class GraphSonBlueRedStorage : AbstractSeparateBlueRedStorage
        {
            private const string GraphFileGraphson = "/Gravegraph.json";

            public override void LoadGraphData(RedisGraph graph, string directory)
            {
                GraphSonReader.InputGraph(graph, string.Concat(directory, GraphFileGraphson));
            }

            public override void SaveGraphData(RedisGraph graph, string directory)
            {
                var filePath = string.Concat(directory, GraphFileGraphson);
                DeleteFile(filePath);
                GraphSonWriter.OutputGraph(graph, filePath, GraphSonMode.EXTENDED);
            }
        }
    }
}