using System.Collections.Generic;
using System.IO;

namespace Frontenac.Blueprints.Impls.TG
{
    /// <summary>
    /// Reads TinkerGraph metadata from a Stream.
    /// </summary>
    public class TinkerMetadataReader
    {
        readonly TinkerGraph _graph;

        public TinkerMetadataReader(TinkerGraph graph)
        {
            _graph = graph;
        }

        /// <summary>
        /// Read TinkerGraph metadata from a file.
        /// </summary>
        /// <param name="filename">the name of the file to read the TinkerGraph metadata from</param>
        public void Load(string filename)
        {
            using (var fos = File.OpenRead(filename))
            {
                Load(fos);
            }
        }

        /// <summary>
        /// Read TinkerGraph metadata from a Stream.
        /// </summary>
        /// <param name="inputStream">the Stream to read the TinkerGraph metadata from</param>
        public void Load(Stream inputStream)
        {
            using (var reader = new BinaryReader(inputStream))
            {
                _graph.CurrentId = reader.ReadInt64();
                ReadIndices(reader, _graph);
                ReadVertexKeyIndices(reader, _graph);
                ReadEdgeKeyIndices(reader, _graph);
            }
        }

        /// <summary>
        /// Read TinkerGraph metadata from a Stream.
        /// </summary>
        /// <param name="graph">the TinkerGraph to push the metadata to</param>
        /// <param name="inputStream">the Stream to read the TinkerGraph metadata from</param>
        public static void Load(TinkerGraph graph, Stream inputStream)
        {
            var reader = new TinkerMetadataReader(graph);
            reader.Load(inputStream);
        }

        /// <summary>
        /// Read TinkerGraph metadata from a file.
        /// </summary>
        /// <param name="graph">the TinkerGraph to push the data to</param>
        /// <param name="filename">the name of the file to read the TinkerGraph metadata from</param>
        public static void Load(TinkerGraph graph, string filename)
        {
            var reader = new TinkerMetadataReader(graph);
            reader.Load(filename);
        }

        void ReadIndices(BinaryReader reader, TinkerGraph graph)
        {
            // Read the number of indices
            int indexCount = reader.ReadInt32();

            for (int i = 0; i < indexCount; i++)
            {
                // Read the index name
                string indexName = reader.ReadString();

                // Read the index type
                byte indexType = reader.ReadByte();

                if (indexType != 1 && indexType != 2)
                {
                    throw new InvalidDataException("Unknown index class type");
                }

                var tinkerIndex = new TinkerIndex(indexName, indexType == 1 ? typeof(IVertex) : typeof(IEdge));

                // Read the number of items associated with this index name
                int indexItemCount = reader.ReadInt32();
                for (int j = 0; j < indexItemCount; j++)
                {
                    // Read the item key
                    string indexItemKey = reader.ReadString();

                    // Read the number of sub-items associated with this item
                    int indexValueItemSetCount = reader.ReadInt32();
                    for (int k = 0; k < indexValueItemSetCount; k++)
                    {
                        // Read the number of vertices or edges in this sub-item
                        int setCount = reader.ReadInt32();
                        for (int l = 0; l < setCount; l++)
                        {
                            // Read the vertex or edge identifier
                            if (indexType == 1)
                            {
                                IVertex v = graph.GetVertex(ReadTypedData(reader));
                                if (v != null)
                                    tinkerIndex.Put(indexItemKey, v.GetProperty(indexItemKey), v);
                            }
                            else if (indexType == 2)
                            {
                                IEdge e = graph.GetEdge(ReadTypedData(reader));
                                if (e != null)
                                    tinkerIndex.Put(indexItemKey, e.GetProperty(indexItemKey), e);
                            }
                        }
                    }
                }

                graph.Indices.Put(indexName, tinkerIndex);
            }
        }

        void ReadVertexKeyIndices(BinaryReader reader, TinkerGraph graph)
        {
            // Read the number of vertex key indices
            int indexCount = reader.ReadInt32();

            for (int i = 0; i < indexCount; i++)
            {
                // Read the key index name
                string indexName = reader.ReadString();

                graph.VertexKeyIndex.CreateKeyIndex(indexName);

                var items = new Dictionary<object, HashSet<IElement>>();

                // Read the number of items associated with this key index name
                int itemCount = reader.ReadInt32();
                for (int j = 0; j < itemCount; j++)
                {
                    // Read the item key
                    object key = ReadTypedData(reader);

                    var vertices = new HashSet<IElement>();

                    // Read the number of vertices in this item
                    int vertexCount = reader.ReadInt32();
                    for (int k = 0; k < vertexCount; k++)
                    {
                        // Read the vertex identifier
                        IVertex v = graph.GetVertex(ReadTypedData(reader));
                        if (v != null)
                            vertices.Add(v);
                    }

                    items.Put(key, vertices);
                }

                graph.VertexKeyIndex.Index.Put(indexName, items);
            }
        }

        void ReadEdgeKeyIndices(BinaryReader reader, TinkerGraph graph)
        {
            // Read the number of edge key indices
            int indexCount = reader.ReadInt32();

            for (int i = 0; i < indexCount; i++)
            {
                // Read the key index name
                string indexName = reader.ReadString();

                graph.EdgeKeyIndex.CreateKeyIndex(indexName);

                var items = new Dictionary<object, HashSet<IElement>>();

                // Read the number of items associated with this key index name
                int itemCount = reader.ReadInt32();
                for (int j = 0; j < itemCount; j++)
                {
                    // Read the item key
                    object key = ReadTypedData(reader);

                    var edges = new HashSet<IElement>();

                    // Read the number of edges in this item
                    int edgeCount = reader.ReadInt32();
                    for (int k = 0; k < edgeCount; k++)
                    {
                        // Read the edge identifier
                        IEdge e = graph.GetEdge(ReadTypedData(reader));
                        if (e != null)
                            edges.Add(e);
                    }

                    items.Put(key, edges);
                }

                graph.EdgeKeyIndex.Index.Put(indexName, items);
            }
        }

        object ReadTypedData(BinaryReader reader)
        {
            byte type = reader.ReadByte();

            if (type == 1)
                return reader.ReadString();
            if (type == 2)
                return reader.ReadInt32();
            if (type == 3)
                return reader.ReadInt64();
            if (type == 4)
                return reader.ReadInt16();
            if (type == 5)
                return reader.ReadSingle();
            if (type == 6)
                return reader.ReadDouble();
            throw new IOException("unknown data type: use java serialization");
        }
    }
}
