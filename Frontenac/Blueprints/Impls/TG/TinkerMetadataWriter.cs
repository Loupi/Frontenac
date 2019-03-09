using System;
using System.IO;

namespace Frontenac.Blueprints.Impls.TG
{
    /// <summary>
    ///     Writes TinkerGraph metadata to an OutputStream.
    /// </summary>
    internal class TinkerMetadataWriter
    {
        private readonly TinkerGraph _tinkerGraph;

        /// <summary>
        ///     the TinkerGraph to pull the data from
        /// </summary>
        /// <param name="tinkerGraph"></param>
        public TinkerMetadataWriter(TinkerGraph tinkerGraph)
        {
            if (tinkerGraph == null)
                throw new ArgumentNullException(nameof(tinkerGraph));

            _tinkerGraph = tinkerGraph;
        }

        /// <summary>
        ///     Write TinkerGraph metadata to a file.
        /// </summary>
        /// <param name="filename">the name of the file to write the TinkerGraph metadata to</param>
        public void Save(string filename)
        {
            if (string.IsNullOrWhiteSpace(filename))
                throw new ArgumentNullException(nameof(filename));

            using (var fos = File.Create(filename))
            {
                Save(fos);
            }
        }

        /// <summary>
        ///     Write TinkerGraph metadata to an OutputStream.
        /// </summary>
        /// <param name="outputStream">the OutputStream to write the TinkerGraph metadata to</param>
        public void Save(Stream outputStream)
        {
            if (outputStream == null)
                throw new ArgumentNullException(nameof(outputStream));

            using (var writer = new BinaryWriter(outputStream))
            {
                writer.Write(_tinkerGraph.CurrentId);
                WriteIndices(writer, _tinkerGraph);
                WriteVertexKeyIndices(writer, _tinkerGraph);
                WriteEdgeKeyIndices(writer, _tinkerGraph);
            }
        }

        /// <summary>
        ///     Write TinkerGraph metadata to an OutputStream.
        /// </summary>
        /// <param name="tinkerGraph">the TinkerGraph to pull the metadata from</param>
        /// <param name="outputStream">the OutputStream to write the TinkerGraph metadata to</param>
        public static void Save(TinkerGraph tinkerGraph, Stream outputStream)
        {
            if (tinkerGraph == null)
                throw new ArgumentNullException(nameof(tinkerGraph));
            if (outputStream == null)
                throw new ArgumentNullException(nameof(outputStream));

            var writer = new TinkerMetadataWriter(tinkerGraph);
            writer.Save(outputStream);
        }

        /// <summary>
        ///     Write TinkerGraph metadata to a file.
        /// </summary>
        /// <param name="tinkerGraph">the TinkerGraph to pull the data from</param>
        /// <param name="filename">the name of the file to write the TinkerGraph metadata to</param>
        public static void Save(TinkerGraph tinkerGraph, string filename)
        {
            if (tinkerGraph == null)
                throw new ArgumentNullException(nameof(tinkerGraph));
            if (string.IsNullOrWhiteSpace(filename))
                throw new ArgumentNullException(nameof(filename));

            var writer = new TinkerMetadataWriter(tinkerGraph);
            writer.Save(filename);
        }

        private static void WriteIndices(BinaryWriter writer, TinkerGraph tinkerGraph)
        {
            if (writer == null)
                throw new ArgumentNullException(nameof(writer));
            if (tinkerGraph == null)
                throw new ArgumentNullException(nameof(tinkerGraph));

            // Write the number of indices
            writer.Write(tinkerGraph.Indices.Count);

            foreach (var index in tinkerGraph.Indices)
            {
                // Write the index name
                writer.Write(index.Key);

                var tinkerIndex = index.Value;
                var indexClass = tinkerIndex.Type;

                // Write the index type
                writer.Write((byte) (indexClass == typeof (IVertex) ? 1 : 2));

                // Write the number of items associated with this index name
                writer.Write(tinkerIndex.Index.Count);
                foreach (var tinkerIndexItem in tinkerIndex.Index)
                {
                    // Write the item key
                    writer.Write(tinkerIndexItem.Key);

                    var tinkerIndexItemSet = tinkerIndexItem.Value;

                    // Write the number of sub-items associated with this item
                    writer.Write(tinkerIndexItemSet.Count);
                    foreach (var items in tinkerIndexItemSet)
                    {
                        if (indexClass == typeof (IVertex))
                        {
                            var vertices = items.Value;

                            // Write the number of vertices in this sub-item
                            writer.Write(vertices.Count);
                            foreach (var v in vertices)
                            {
                                // Write the vertex identifier
                                WriteTypedData(writer, v.Value.Id);
                            }
                        }
                        else if (indexClass == typeof (IEdge))
                        {
                            var edges = items.Value;

                            // Write the number of edges in this sub-item
                            writer.Write(edges.Count);
                            foreach (var e in edges)
                            {
                                // Write the edge identifier
                                WriteTypedData(writer, e.Value.Id);
                            }
                        }
                    }
                }
            }
        }

        private static void WriteVertexKeyIndices(BinaryWriter writer, TinkerGraph tinkerGraph)
        {
            if (writer == null)
                throw new ArgumentNullException(nameof(writer));
            if (tinkerGraph == null)
                throw new ArgumentNullException(nameof(tinkerGraph));

            // Write the number of vertex key indices
            writer.Write(tinkerGraph.VertexKeyIndex.Index.Count);

            foreach (var index in tinkerGraph.VertexKeyIndex.Index)
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
                    foreach (var v in item.Value)
                    {
                        // Write the vertex identifier
                        WriteTypedData(writer, v.Value.Id);
                    }
                }
            }
        }

        private static void WriteEdgeKeyIndices(BinaryWriter writer, TinkerGraph tinkerGraph)
        {
            if (writer == null)
                throw new ArgumentNullException(nameof(writer));
            if (tinkerGraph == null)
                throw new ArgumentNullException(nameof(tinkerGraph));

            // Write the number of edge key indices
            writer.Write(tinkerGraph.EdgeKeyIndex.Index.Count);

            foreach (var index in tinkerGraph.EdgeKeyIndex.Index)
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
                    foreach (var e in item.Value)
                    {
                        // Write the edge identifier
                        WriteTypedData(writer, e.Value.Id);
                    }
                }
            }
        }

        private static void WriteTypedData(BinaryWriter writer, object data)
        {
            if (writer == null)
                throw new ArgumentNullException(nameof(writer));
            if (data == null)
                throw new ArgumentNullException(nameof(data));

            var s = data as string;
            if (s != null)
            {
                writer.Write((byte) 1);
                writer.Write(s);
            }
            else if (data is int)
            {
                writer.Write((byte) 2);
                writer.Write((int) data);
            }
            else if (data is long)
            {
                writer.Write((byte) 3);
                writer.Write((long) data);
            }
            else if (data is short)
            {
                writer.Write((byte) 4);
                writer.Write((short) data);
            }
            else if (data is float)
            {
                writer.Write((byte) 5);
                writer.Write((float) data);
            }
            else if (data is double)
            {
                writer.Write((byte) 6);
                writer.Write((double) data);
            }
            else
                throw new IOException("unknown data type: use .NET serialization");
        }
    }
}