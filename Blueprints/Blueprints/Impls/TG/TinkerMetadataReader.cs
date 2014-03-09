using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.IO;

namespace Frontenac.Blueprints.Impls.TG
{
    /// <summary>
    ///     Reads TinkerGraph metadata from a Stream.
    /// </summary>
    public class TinkerMetadataReader
    {
        private readonly TinkerGraph _graph;

        public TinkerMetadataReader(TinkerGraph graph)
        {
            Contract.Requires(graph != null);

            _graph = graph;
        }

        /// <summary>
        ///     Read TinkerGraph metadata from a file.
        /// </summary>
        /// <param name="filename">the name of the file to read the TinkerGraph metadata from</param>
        public void Load(string filename)
        {
            Contract.Requires(!string.IsNullOrWhiteSpace(filename));

            using (var fos = File.OpenRead(filename))
            {
                Load(fos);
            }
        }

        /// <summary>
        ///     Read TinkerGraph metadata from a Stream.
        /// </summary>
        /// <param name="inputStream">the Stream to read the TinkerGraph metadata from</param>
        public void Load(Stream inputStream)
        {
            Contract.Requires(inputStream != null);

            using (var reader = new BinaryReader(inputStream))
            {
                _graph.CurrentId = reader.ReadInt64();
                ReadIndices(reader, _graph);
                ReadVertexKeyIndices(reader, _graph);
                ReadEdgeKeyIndices(reader, _graph);
            }
        }

        /// <summary>
        ///     Read TinkerGraph metadata from a Stream.
        /// </summary>
        /// <param name="graph">the TinkerGraph to push the metadata to</param>
        /// <param name="inputStream">the Stream to read the TinkerGraph metadata from</param>
        public static void Load(TinkerGraph graph, Stream inputStream)
        {
            Contract.Requires(graph != null);
            Contract.Requires(inputStream != null);

            var reader = new TinkerMetadataReader(graph);
            reader.Load(inputStream);
        }

        /// <summary>
        ///     Read TinkerGraph metadata from a file.
        /// </summary>
        /// <param name="graph">the TinkerGraph to push the data to</param>
        /// <param name="filename">the name of the file to read the TinkerGraph metadata from</param>
        public static void Load(TinkerGraph graph, string filename)
        {
            Contract.Requires(graph != null);
            Contract.Requires(!string.IsNullOrWhiteSpace(filename));

            var reader = new TinkerMetadataReader(graph);
            reader.Load(filename);
        }

        private static void ReadIndices(BinaryReader reader, TinkerGraph graph)
        {
            Contract.Requires(reader != null);
            Contract.Requires(graph != null);

            // Read the number of indices
            var indexCount = reader.ReadInt32();

            for (var i = 0; i < indexCount; i++)
            {
                // Read the index name
                var indexName = reader.ReadString();

                // Read the index type
                var indexType = reader.ReadByte();

                if (indexType != 1 && indexType != 2)
                {
                    throw new InvalidDataException("Unknown index class type");
                }

                var tinkerIndex = new TinkerIndex(indexName, indexType == 1 ? typeof (IVertex) : typeof (IEdge));

                // Read the number of items associated with this index name
                var indexItemCount = reader.ReadInt32();
                for (var j = 0; j < indexItemCount; j++)
                {
                    // Read the item key
                    var indexItemKey = reader.ReadString();

                    // Read the number of sub-items associated with this item
                    var indexValueItemSetCount = reader.ReadInt32();
                    for (var k = 0; k < indexValueItemSetCount; k++)
                    {
                        // Read the number of vertices or edges in this sub-item
                        var setCount = reader.ReadInt32();
                        for (var l = 0; l < setCount; l++)
                        {
                            // Read the vertex or edge identifier
                            if (indexType == 1)
                            {
                                var v = graph.GetVertex(ReadTypedData(reader));
                                if (v != null)
                                    tinkerIndex.Put(indexItemKey, v.GetProperty(indexItemKey), v);
                            }
                            else if (indexType == 2)
                            {
                                var e = graph.GetEdge(ReadTypedData(reader));
                                if (e != null)
                                    tinkerIndex.Put(indexItemKey, e.GetProperty(indexItemKey), e);
                            }
                        }
                    }
                }

                graph.Indices.Put(indexName, tinkerIndex);
            }
        }

        private static void ReadVertexKeyIndices(BinaryReader reader, TinkerGraph graph)
        {
            Contract.Requires(reader != null);
            Contract.Requires(graph != null);

            // Read the number of vertex key indices
            var indexCount = reader.ReadInt32();

            for (var i = 0; i < indexCount; i++)
            {
                // Read the key index name
                var indexName = reader.ReadString();

                graph.VertexKeyIndex.CreateKeyIndex(indexName);

                var items = new Dictionary<object, HashSet<IElement>>();

                // Read the number of items associated with this key index name
                var itemCount = reader.ReadInt32();
                for (var j = 0; j < itemCount; j++)
                {
                    // Read the item key
                    var key = ReadTypedData(reader);

                    var vertices = new HashSet<IElement>();

                    // Read the number of vertices in this item
                    var vertexCount = reader.ReadInt32();
                    for (var k = 0; k < vertexCount; k++)
                    {
                        // Read the vertex identifier
                        var v = graph.GetVertex(ReadTypedData(reader));
                        if (v != null)
                            vertices.Add(v);
                    }

                    items.Put(key, vertices);
                }

                graph.VertexKeyIndex.Index.Put(indexName, items);
            }
        }

        private static void ReadEdgeKeyIndices(BinaryReader reader, TinkerGraph graph)
        {
            Contract.Requires(reader != null);
            Contract.Requires(graph != null);

            // Read the number of edge key indices
            var indexCount = reader.ReadInt32();

            for (var i = 0; i < indexCount; i++)
            {
                // Read the key index name
                var indexName = reader.ReadString();

                graph.EdgeKeyIndex.CreateKeyIndex(indexName);

                var items = new Dictionary<object, HashSet<IElement>>();

                // Read the number of items associated with this key index name
                var itemCount = reader.ReadInt32();
                for (var j = 0; j < itemCount; j++)
                {
                    // Read the item key
                    var key = ReadTypedData(reader);

                    var edges = new HashSet<IElement>();

                    // Read the number of edges in this item
                    var edgeCount = reader.ReadInt32();
                    for (var k = 0; k < edgeCount; k++)
                    {
                        // Read the edge identifier
                        var e = graph.GetEdge(ReadTypedData(reader));
                        if (e != null)
                            edges.Add(e);
                    }

                    items.Put(key, edges);
                }

                graph.EdgeKeyIndex.Index.Put(indexName, items);
            }
        }

        private static object ReadTypedData(BinaryReader reader)
        {
            Contract.Requires(reader != null);
            Contract.Ensures(Contract.Result<object>() != null);

            var type = reader.ReadByte();

            switch (type)
            {
                case 1:
                    return reader.ReadString();
                case 2:
                    return reader.ReadInt32();
                case 3:
                    return reader.ReadInt64();
                case 4:
                    return reader.ReadInt16();
                case 5:
                    return reader.ReadSingle();
                case 6:
                    return reader.ReadDouble();
                default:
                    throw new IOException("unknown data type: use .NET serialization");
            }
        }
    }
}