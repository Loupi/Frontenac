using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using Frontenac.Blueprints.Util.IO.GML;
using Frontenac.Blueprints.Util.IO.GraphML;
using Frontenac.Blueprints.Util.IO.GraphSON;
using Grave;

namespace Grave_test
{
    /// <summary>
    ///     Constructs GraveFile instances to load and save GraveGraph instances.
    /// </summary>
    public class GraveGraphStorageFactory
    {
        private static GraveGraphStorageFactory _factory;

        private GraveGraphStorageFactory()
        {
        }

        public static GraveGraphStorageFactory GetInstance()
        {
            return _factory ?? (_factory = new GraveGraphStorageFactory());
        }

        public IGraveStorage GetGraveStorage(FileType fileType)
        {
            switch (fileType)
            {
                case FileType.Gml:
                    return new GmlGraveStorage();
                case FileType.Graphml:
                    return new GraphMlGraveStorage();
                case FileType.Graphson:
                    return new GraphSonGraveStorage();
                case FileType.DotNet:
                    return new DotNetGraveStorage();
            }

            throw new Exception(string.Format("File Type {0} is not configurable by the factory", fileType));
        }

        /// <summary>
        ///     Base class for loading and saving a GraveGraph.
        /// </summary>
        private abstract class AbstractGraveStorage : IGraveStorage
        {
            public abstract GraveGraph Load(string directory);
            public abstract void Save(GraveGraph graph, string directory);

            /// <summary>
            ///     Clean up the directory that houses the GraveGraph.
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
        ///     meta data stored in the GraveGraph.
        /// </summary>
        private abstract class AbstractSeparateGraveStorage : AbstractGraveStorage
        {
            /// <summary>
            ///     Save the data of the graph with the specific file format of the implementation.
            /// </summary>
            public abstract void SaveGraphData(GraveGraph graph, string directory);

            /// <summary>
            ///     Load the data from the graph with the specific file format of the implementation.
            /// </summary>
            public abstract void LoadGraphData(GraveGraph graph, string directory);

            public override GraveGraph Load(string directory)
            {
                if (!Directory.Exists(directory))
                    throw new Exception(string.Concat("Directory ", directory, " does not exist"));

                var graph = GraveFactory.CreateGraph();
                LoadGraphData(graph, directory);

                return graph;
            }

            public override void Save(GraveGraph graph, string directory)
            {
                if (!Directory.Exists(directory))
                    Directory.CreateDirectory(directory);

                SaveGraphData(graph, directory);
            }
        }

        /// <summary>
        ///     Reads and writes a GraveGraph using .NET object serialization.
        /// </summary>
        private class DotNetGraveStorage : AbstractGraveStorage
        {
            private const string GraphFileDotNet = "/tinkergraph.dat";

            public override GraveGraph Load(string directory)
            {
                using (var stream = File.OpenRead(string.Concat(directory, GraphFileDotNet)))
                {
                    var formatter = new BinaryFormatter();
                    return (GraveGraph) formatter.Deserialize(stream);
                }
            }

            public override void Save(GraveGraph graph, string directory)
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
        private class GmlGraveStorage : AbstractSeparateGraveStorage
        {
            private const string GraphFileGml = "/Gravegraph.gml";

            public override void LoadGraphData(GraveGraph graph, string directory)
            {
                GmlReader.InputGraph(graph, string.Concat(directory, GraphFileGml));
            }

            public override void SaveGraphData(GraveGraph graph, string directory)
            {
                var filePath = string.Concat(directory, GraphFileGml);
                DeleteFile(filePath);
                GmlWriter.OutputGraph(graph, filePath);
            }
        }

        /// <summary>
        ///     Reads and writes a GraveGraph to GraphML as the format for the data.
        /// </summary>
        private class GraphMlGraveStorage : AbstractSeparateGraveStorage
        {
            private const string GraphFileGraphml = "/Gravegraph.xml";

            public override void LoadGraphData(GraveGraph graph, string directory)
            {
                GraphMlReader.InputGraph(graph, string.Concat(directory, GraphFileGraphml));
            }

            public override void SaveGraphData(GraveGraph graph, string directory)
            {
                var filePath = string.Concat(directory, GraphFileGraphml);
                DeleteFile(filePath);
                GraphMlWriter.OutputGraph(graph, filePath);
            }
        }

        /// <summary>
        ///     Reads and writes a GraveGraph to GraphSON as the format for the data.
        /// </summary>
        private class GraphSonGraveStorage : AbstractSeparateGraveStorage
        {
            private const string GraphFileGraphson = "/Gravegraph.json";

            public override void LoadGraphData(GraveGraph graph, string directory)
            {
                GraphSonReader.InputGraph(graph, string.Concat(directory, GraphFileGraphson));
            }

            public override void SaveGraphData(GraveGraph graph, string directory)
            {
                var filePath = string.Concat(directory, GraphFileGraphson);
                DeleteFile(filePath);
                GraphSonWriter.OutputGraph(graph, filePath, GraphSonMode.EXTENDED);
            }
        }
    }
}