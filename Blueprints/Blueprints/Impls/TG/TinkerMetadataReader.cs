using System.Collections.Concurrent;
using System.Diagnostics.Contracts;
using System.IO;

namespace Frontenac.Blueprints.Impls.TG
{
    /// <summary>
    ///     Reads TinkerGraĥ metadata from a Stream.
    /// </summary>
    public class TinkerMetadataReader
    {
        private readonly TinkerGraĥ _tinkerGraĥ;

        public TinkerMetadataReader(TinkerGraĥ tinkerGraĥ)
        {
            Contract.Requires(tinkerGraĥ != null);

            _tinkerGraĥ = tinkerGraĥ;
        }

        /// <summary>
        ///     Read TinkerGraĥ metadata from a file.
        /// </summary>
        /// <param name="filename">the name of the file to read the TinkerGraĥ metadata from</param>
        public void Load(string filename)
        {
            Contract.Requires(!string.IsNullOrWhiteSpace(filename));

            using (var fos = File.OpenRead(filename))
            {
                Load(fos);
            }
        }

        /// <summary>
        ///     Read TinkerGraĥ metadata from a Stream.
        /// </summary>
        /// <param name="inputStream">the Stream to read the TinkerGraĥ metadata from</param>
        public void Load(Stream inputStream)
        {
            Contract.Requires(inputStream != null);

            using (var reader = new BinaryReader(inputStream))
            {
                _tinkerGraĥ.CurrentId = reader.ReadInt64();
                ReadIndices(reader, _tinkerGraĥ);
                ReadVertexKeyIndices(reader, _tinkerGraĥ);
                ReadEdgeKeyIndices(reader, _tinkerGraĥ);
            }
        }

        /// <summary>
        ///     Read TinkerGraĥ metadata from a Stream.
        /// </summary>
        /// <param name="tinkerGraĥ">the IGraph to push the metadata to</param>
        /// <param name="inputStream">the Stream to read the TinkerGraĥ metadata from</param>
        public static void Load(TinkerGraĥ tinkerGraĥ, Stream inputStream)
        {
            Contract.Requires(tinkerGraĥ != null);
            Contract.Requires(inputStream != null);

            var reader = new TinkerMetadataReader(tinkerGraĥ);
            reader.Load(inputStream);
        }

        /// <summary>
        ///     Read TinkerGraĥ metadata from a file.
        /// </summary>
        /// <param name="tinkerGraĥ">the TinkerGraĥ to push the data to</param>
        /// <param name="filename">the name of the file to read the TinkerGraĥ metadata from</param>
        public static void Load(TinkerGraĥ tinkerGraĥ, string filename)
        {
            Contract.Requires(tinkerGraĥ != null);
            Contract.Requires(!string.IsNullOrWhiteSpace(filename));

            var reader = new TinkerMetadataReader(tinkerGraĥ);
            reader.Load(filename);
        }

        private static void ReadIndices(BinaryReader reader, TinkerGraĥ tinkerGraĥ)
        {
            Contract.Requires(reader != null);
            Contract.Requires(tinkerGraĥ != null);

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
                                var v = tinkerGraĥ.GetVertex(ReadTypedData(reader));
                                if (v != null)
                                    tinkerIndex.Put(indexItemKey, v.GetProperty(indexItemKey), v);
                            }
                            else if (indexType == 2)
                            {
                                var e = tinkerGraĥ.GetEdge(ReadTypedData(reader));
                                if (e != null)
                                    tinkerIndex.Put(indexItemKey, e.GetProperty(indexItemKey), e);
                            }
                        }
                    }
                }

                tinkerGraĥ.Indices.Put(indexName, tinkerIndex);
            }
        }

        private static void ReadVertexKeyIndices(BinaryReader reader, TinkerGraĥ tinkerGraĥ)
        {
            Contract.Requires(reader != null);
            Contract.Requires(tinkerGraĥ != null);

            // Read the number of vertex key indices
            var indexCount = reader.ReadInt32();

            for (var i = 0; i < indexCount; i++)
            {
                // Read the key index name
                var indexName = reader.ReadString();

                tinkerGraĥ.VertexKeyIndex.CreateKeyIndex(indexName);

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
                        var v = tinkerGraĥ.GetVertex(ReadTypedData(reader));
                        if (v != null)
                            vertices.TryAdd(v.Id.ToString(), v);
                    }

                    items.Put(key, vertices);
                }

                tinkerGraĥ.VertexKeyIndex.Index.Put(indexName, items);
            }
        }

        private static void ReadEdgeKeyIndices(BinaryReader reader, TinkerGraĥ tinkerGraĥ)
        {
            Contract.Requires(reader != null);
            Contract.Requires(tinkerGraĥ != null);

            // Read the number of edge key indices
            var indexCount = reader.ReadInt32();

            for (var i = 0; i < indexCount; i++)
            {
                // Read the key index name
                var indexName = reader.ReadString();

                tinkerGraĥ.EdgeKeyIndex.CreateKeyIndex(indexName);

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
                        var e = tinkerGraĥ.GetEdge(ReadTypedData(reader));
                        if (e != null)
                            edges.TryAdd(e.Id.ToString(), e);
                    }

                    items.Put(key, edges);
                }

                tinkerGraĥ.EdgeKeyIndex.Index.Put(indexName, items);
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