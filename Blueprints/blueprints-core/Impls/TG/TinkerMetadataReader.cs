using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
        public void load(string filename)
        {
            using (var fos = File.OpenRead(filename))
            {
                load(fos);
            }
        }

        /// <summary>
        /// Read TinkerGraph metadata from a Stream.
        /// </summary>
        /// <param name="inputStream">the Stream to read the TinkerGraph metadata from</param>
        public void load(Stream inputStream)
        {
            using (BinaryReader reader = new BinaryReader(inputStream))
            {
                _graph.currentId = reader.ReadInt64();
                readIndices(reader, _graph);
                readVertexKeyIndices(reader, _graph);
                readEdgeKeyIndices(reader, _graph);
            }
        }

        /// <summary>
        /// Read TinkerGraph metadata from a Stream.
        /// </summary>
        /// <param name="graph">the TinkerGraph to push the metadata to</param>
        /// <param name="inputStream">the Stream to read the TinkerGraph metadata from</param>
        public static void load(TinkerGraph graph, Stream inputStream)
        {
            TinkerMetadataReader reader = new TinkerMetadataReader(graph);
            reader.load(inputStream);
        }

        /// <summary>
        /// Read TinkerGraph metadata from a file.
        /// </summary>
        /// <param name="graph">the TinkerGraph to push the data to</param>
        /// <param name="filename">the name of the file to read the TinkerGraph metadata from</param>
        public static void load(TinkerGraph graph, string filename)
        {
            TinkerMetadataReader reader = new TinkerMetadataReader(graph);
            reader.load(filename);
        }

        void readIndices(BinaryReader reader, TinkerGraph graph)
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

                TinkerIndex tinkerIndex = new TinkerIndex(indexName, indexType == 1 ? typeof(Vertex) : typeof(Edge));

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
                                Vertex v = graph.getVertex(readTypedData(reader));
                                if (v != null)
                                    tinkerIndex.put(indexItemKey, v.getProperty(indexItemKey), v);
                            }
                            else if (indexType == 2)
                            {
                                Edge e = graph.getEdge(readTypedData(reader));
                                if (e != null)
                                    tinkerIndex.put(indexItemKey, e.getProperty(indexItemKey), e);
                            }
                        }
                    }
                }

                graph.indices.put(indexName, tinkerIndex);
            }
        }

        void readVertexKeyIndices(BinaryReader reader, TinkerGraph graph)
        {
            // Read the number of vertex key indices
            int indexCount = reader.ReadInt32();

            for (int i = 0; i < indexCount; i++)
            {
                // Read the key index name
                string indexName = reader.ReadString();

                graph.vertexKeyIndex.createKeyIndex(indexName);

                Dictionary<object, HashSet<Element>> items = new Dictionary<object, HashSet<Element>>();

                // Read the number of items associated with this key index name
                int itemCount = reader.ReadInt32();
                for (int j = 0; j < itemCount; j++)
                {
                    // Read the item key
                    object key = readTypedData(reader);

                    HashSet<Element> vertices = new HashSet<Element>();

                    // Read the number of vertices in this item
                    int vertexCount = reader.ReadInt32();
                    for (int k = 0; k < vertexCount; k++)
                    {
                        // Read the vertex identifier
                        Vertex v = graph.getVertex(readTypedData(reader));
                        if (v != null)
                            vertices.Add((TinkerVertex)v);
                    }

                    items.put(key, vertices);
                }

                graph.vertexKeyIndex.index.put(indexName, items);
            }
        }

        void readEdgeKeyIndices(BinaryReader reader, TinkerGraph graph)
        {
            // Read the number of edge key indices
            int indexCount = reader.ReadInt32();

            for (int i = 0; i < indexCount; i++)
            {
                // Read the key index name
                string indexName = reader.ReadString();

                graph.edgeKeyIndex.createKeyIndex(indexName);

                Dictionary<object, HashSet<Element>> items = new Dictionary<object, HashSet<Element>>();

                // Read the number of items associated with this key index name
                int itemCount = reader.ReadInt32();
                for (int j = 0; j < itemCount; j++)
                {
                    // Read the item key
                    object key = readTypedData(reader);

                    HashSet<Element> edges = new HashSet<Element>();

                    // Read the number of edges in this item
                    int edgeCount = reader.ReadInt32();
                    for (int k = 0; k < edgeCount; k++)
                    {
                        // Read the edge identifier
                        Edge e = graph.getEdge(readTypedData(reader));
                        if (e != null)
                            edges.Add((TinkerEdge)e);
                    }

                    items.put(key, edges);
                }

                graph.edgeKeyIndex.index.put(indexName, items);
            }
        }

        object readTypedData(BinaryReader reader)
        {
            byte type = reader.ReadByte();

            if (type == 1)
                return reader.ReadString();
            else if (type == 2)
                return reader.ReadInt32();
            else if (type == 3)
                return reader.ReadInt64();
            else if (type == 4)
                return reader.ReadInt16();
            else if (type == 5)
                return reader.ReadSingle();
            else if (type == 6)
                return reader.ReadDouble();
            else
                throw new IOException("unknown data type: use java serialization");
        }
    }
}
