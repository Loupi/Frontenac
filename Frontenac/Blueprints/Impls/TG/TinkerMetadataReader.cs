using System;
using System.Collections.Concurrent;
using System.IO;

namespace Frontenac.Blueprints.Impls.TG
{
    /// <summary>
    ///     Reads TinkerGraph metadata from a Stream.
    /// </summary>
    public class TinkerMetadataReader
    {
        private readonly TinkerGraph _tinkerGraph;

        public TinkerMetadataReader(TinkerGraph tinkerGraph)
        {
            if (tinkerGraph == null)
                throw new ArgumentNullException(nameof(tinkerGraph));

            _tinkerGraph = tinkerGraph;
        }

        /// <summary>
        ///     Read TinkerGraph metadata from a file.
        /// </summary>
        /// <param name="filename">the name of the file to read the TinkerGraph metadata from</param>
        public void Load(string filename)
        {
            if (string.IsNullOrWhiteSpace(filename))
                throw new ArgumentNullException(nameof(filename));

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
            if (inputStream == null)
                throw new ArgumentNullException(nameof(inputStream));

            using (var reader = new BinaryReader(inputStream))
            {
                _tinkerGraph.CurrentId = reader.ReadInt64();
                ReadIndices(reader, _tinkerGraph);
                ReadVertexKeyIndices(reader, _tinkerGraph);
                ReadEdgeKeyIndices(reader, _tinkerGraph);
            }
        }

        /// <summary>
        ///     Read TinkerGraph metadata from a Stream.
        /// </summary>
        /// <param name="tinkerGraph">the IGraph to push the metadata to</param>
        /// <param name="inputStream">the Stream to read the TinkerGraph metadata from</param>
        public static void Load(TinkerGraph tinkerGraph, Stream inputStream)
        {
            if (tinkerGraph == null)
                throw new ArgumentNullException(nameof(tinkerGraph));
            if (inputStream == null)
                throw new ArgumentNullException(nameof(inputStream));

            var reader = new TinkerMetadataReader(tinkerGraph);
            reader.Load(inputStream);
        }

        /// <summary>
        ///     Read TinkerGraph metadata from a file.
        /// </summary>
        /// <param name="tinkerGraph">the TinkerGraph to push the data to</param>
        /// <param name="filename">the name of the file to read the TinkerGraph metadata from</param>
        public static void Load(TinkerGraph tinkerGraph, string filename)
        {
            if (tinkerGraph == null)
                throw new ArgumentNullException(nameof(tinkerGraph));
            if (string.IsNullOrWhiteSpace(filename))
                throw new ArgumentNullException(nameof(filename));

            var reader = new TinkerMetadataReader(tinkerGraph);
            reader.Load(filename);
        }

        private static void ReadIndices(BinaryReader reader, TinkerGraph tinkerGraph)
        {
            if (reader == null)
                throw new ArgumentNullException(nameof(reader));
            if (tinkerGraph == null)
                throw new ArgumentNullException(nameof(tinkerGraph));

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
                                var v = tinkerGraph.GetVertex(ReadTypedData(reader));
                                if (v != null)
                                    tinkerIndex.Put(indexItemKey, v.GetProperty(indexItemKey), v);
                            }
                            else if (indexType == 2)
                            {
                                var e = tinkerGraph.GetEdge(ReadTypedData(reader));
                                if (e != null)
                                    tinkerIndex.Put(indexItemKey, e.GetProperty(indexItemKey), e);
                            }
                        }
                    }
                }

                tinkerGraph.Indices.Put(indexName, tinkerIndex);
            }
        }

        private static void ReadVertexKeyIndices(BinaryReader reader, TinkerGraph tinkerGraph)
        {
            if (reader == null)
                throw new ArgumentNullException(nameof(reader));
            if (tinkerGraph == null)
                throw new ArgumentNullException(nameof(tinkerGraph));

            // Read the number of vertex key indices
            var indexCount = reader.ReadInt32();

            for (var i = 0; i < indexCount; i++)
            {
                // Read the key index name
                var indexName = reader.ReadString();

                tinkerGraph.VertexKeyIndex.CreateKeyIndex(indexName);

                var items = new ConcurrentDictionary<object, ConcurrentDictionary<string, IElement>>();

                // Read the number of items associated with this key index name
                var itemCount = reader.ReadInt32();
                for (var j = 0; j < itemCount; j++)
                {
                    // Read the item key
                    var key = ReadTypedData(reader);

                    var vertices = new ConcurrentDictionary<string, IElement>();

                    // Read the number of vertices in this item
                    var vertexCount = reader.ReadInt32();
                    for (var k = 0; k < vertexCount; k++)
                    {
                        // Read the vertex identifier
                        var v = tinkerGraph.GetVertex(ReadTypedData(reader));
                        if (v != null)
                            vertices.TryAdd(v.Id.ToString(), v);
                    }

                    items.Put(key, vertices);
                }

                tinkerGraph.VertexKeyIndex.Index.Put(indexName, items);
            }
        }

        private static void ReadEdgeKeyIndices(BinaryReader reader, TinkerGraph tinkerGraph)
        {
            if (reader == null)
                throw new ArgumentNullException(nameof(reader));
            if (tinkerGraph == null)
                throw new ArgumentNullException(nameof(tinkerGraph));

            // Read the number of edge key indices
            var indexCount = reader.ReadInt32();

            for (var i = 0; i < indexCount; i++)
            {
                // Read the key index name
                var indexName = reader.ReadString();

                tinkerGraph.EdgeKeyIndex.CreateKeyIndex(indexName);

                var items = new ConcurrentDictionary<object, ConcurrentDictionary<string, IElement>>();

                // Read the number of items associated with this key index name
                var itemCount = reader.ReadInt32();
                for (var j = 0; j < itemCount; j++)
                {
                    // Read the item key
                    var key = ReadTypedData(reader);

                    var edges = new ConcurrentDictionary<string, IElement>();

                    // Read the number of edges in this item
                    var edgeCount = reader.ReadInt32();
                    for (var k = 0; k < edgeCount; k++)
                    {
                        // Read the edge identifier
                        var e = tinkerGraph.GetEdge(ReadTypedData(reader));
                        if (e != null)
                            edges.TryAdd(e.Id.ToString(), e);
                    }

                    items.Put(key, edges);
                }

                tinkerGraph.EdgeKeyIndex.Index.Put(indexName, items);
            }
        }

        private static object ReadTypedData(BinaryReader reader)
        {
            if (reader == null)
                throw new ArgumentNullException(nameof(reader));

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