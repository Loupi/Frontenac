using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Frontenac.Blueprints.Impls.TG
{
    /// <summary>
    /// Writes TinkerGraph metadata to an OutputStream.
    /// </summary>
    class TinkerMetadataWriter
    {
        readonly TinkerGraph _Graph;

        /// <summary>
        /// the TinkerGraph to pull the data from
        /// </summary>
        /// <param name="graph"></param>
        public TinkerMetadataWriter(TinkerGraph graph)
        {
            _Graph = graph;
        }

        /// <summary>
        /// Write TinkerGraph metadata to a file.
        /// </summary>
        /// <param name="filename">the name of the file to write the TinkerGraph metadata to</param>
        public void Save(string filename)
        {
            using (var fos = File.Create(filename))
            {
                Save(fos);
            }
        }

        /// <summary>
        /// Write TinkerGraph metadata to an OutputStream.
        /// </summary>
        /// <param name="outputStream">the OutputStream to write the TinkerGraph metadata to</param>
        public void Save(Stream outputStream)
        {
            using (BinaryWriter writer = new BinaryWriter(outputStream))
            {
                writer.Write(this._Graph._CurrentId);
                WriteIndices(writer, _Graph);
                WriteVertexKeyIndices(writer, _Graph);
                WriteEdgeKeyIndices(writer, _Graph);
            }
        }

        /// <summary>
        /// Write TinkerGraph metadata to an OutputStream.
        /// </summary>
        /// <param name="graph">the TinkerGraph to pull the metadata from</param>
        /// <param name="outputStream">the OutputStream to write the TinkerGraph metadata to</param>
        public static void Save(TinkerGraph graph, Stream outputStream)
        {
            TinkerMetadataWriter writer = new TinkerMetadataWriter(graph);
            writer.Save(outputStream);
        }

        /// <summary>
        /// Write TinkerGraph metadata to a file.
        /// </summary>
        /// <param name="graph">the TinkerGraph to pull the data from</param>
        /// <param name="filename">the name of the file to write the TinkerGraph metadata to</param>
        public static void Save(TinkerGraph graph, string filename)
        {
            TinkerMetadataWriter writer = new TinkerMetadataWriter(graph);
            writer.Save(filename);
        }

        void WriteIndices(BinaryWriter writer, TinkerGraph graph)
        {
            // Write the number of indices
            writer.Write(graph._Indices.Count);

            foreach (var index in graph._Indices)
            {
                // Write the index name
                writer.Write(index.Key);

                TinkerIndex tinkerIndex = index.Value;
                Type indexClass = tinkerIndex.GetIndexClass();

                // Write the index type
                writer.Write((byte)(indexClass == typeof(Vertex) ? 1 : 2));

                // Write the number of items associated with this index name
                writer.Write(tinkerIndex._Index.Count);
                foreach (var tinkerIndexItem in tinkerIndex._Index)
                {
                    // Write the item key
                    writer.Write(tinkerIndexItem.Key);

                    var tinkerIndexItemSet = tinkerIndexItem.Value;

                    // Write the number of sub-items associated with this item
                    writer.Write(tinkerIndexItemSet.Count);
                    foreach (var items in tinkerIndexItemSet)
                    {
                        if (indexClass == typeof(Vertex))
                        {
                            var vertices = items.Value;

                            // Write the number of vertices in this sub-item
                            writer.Write(vertices.Count);
                            foreach (Vertex v in vertices)
                            {
                                // Write the vertex identifier
                                WriteTypedData(writer, v.GetId());
                            }
                        }
                        else if (indexClass == typeof(Edge))
                        {
                            var edges = items.Value;

                            // Write the number of edges in this sub-item
                            writer.Write(edges.Count);
                            foreach (Edge e in edges)
                            {
                                // Write the edge identifier
                                WriteTypedData(writer, e.GetId());
                            }
                        }
                    }
                }
            }
        }

        void WriteVertexKeyIndices(BinaryWriter writer, TinkerGraph graph)
        {
            // Write the number of vertex key indices
            writer.Write(graph._VertexKeyIndex._Index.Count);

            foreach (var index in graph._VertexKeyIndex._Index)
            {
                // Write the key index name
                writer.Write(index.Key);

                // Write the number of items associated with this key index name
                writer.Write(index.Value.Count);
                foreach (var item in index.Value)
                {
                    // Write the item key
                    WriteTypedData(writer, item.Key);

                    // Write the number of vertices in this item
                    writer.Write(item.Value.Count);
                    foreach (Vertex v in item.Value)
                    {
                        // Write the vertex identifier
                        WriteTypedData(writer, v.GetId());
                    }
                }
            }
        }

        void WriteEdgeKeyIndices(BinaryWriter writer, TinkerGraph graph)
        {
            // Write the number of edge key indices
            writer.Write(graph._EdgeKeyIndex._Index.Count);

            foreach (var index in graph._EdgeKeyIndex._Index)
            {
                // Write the key index name
                writer.Write(index.Key);

                // Write the number of items associated with this key index name
                writer.Write(index.Value.Count);
                foreach (var item in index.Value)
                {
                    // Write the item key
                    WriteTypedData(writer, item.Key);

                    // Write the number of edges in this item
                    writer.Write(item.Value.Count);
                    foreach (Edge e in item.Value)
                    {
                        // Write the edge identifier
                        WriteTypedData(writer, e.GetId());
                    }
                }
            }
        }

        void WriteTypedData(BinaryWriter writer, object data)
        {
            if (data is string)
            {
                writer.Write((byte)1);
                writer.Write((string)data);
            }
            else if (data is int)
            {
                writer.Write((byte)2);
                writer.Write((int)data);
            }
            else if (data is long)
            {
                writer.Write((byte)3);
                writer.Write((long)data);
            }
            else if (data is short)
            {
                writer.Write((byte)4);
                writer.Write((short)data);
            }
            else if (data is float)
            {
                writer.Write((byte)5);
                writer.Write((float)data);
            }
            else if (data is double)
            {
                writer.Write((byte)6);
                writer.Write((double)data);
            }
            else
                throw new IOException("unknown data type: use java serialization");
        }
    }
}
