using System.Diagnostics.Contracts;
using System.IO;

namespace Frontenac.Blueprints.Impls.TG
{
    /// <summary>
    ///     Writes TinkerGraph metadata to an OutputStream.
    /// </summary>
    internal class TinkerMetadataWriter
    {
        private readonly TinkerGraph _graph;

        /// <summary>
        ///     the TinkerGraph to pull the data from
        /// </summary>
        /// <param name="graph"></param>
        public TinkerMetadataWriter(TinkerGraph graph)
        {
            Contract.Requires(graph != null);

            _graph = graph;
        }

        /// <summary>
        ///     Write TinkerGraph metadata to a file.
        /// </summary>
        /// <param name="filename">the name of the file to write the TinkerGraph metadata to</param>
        public void Save(string filename)
        {
            Contract.Requires(!string.IsNullOrWhiteSpace(filename));

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
            Contract.Requires(outputStream != null);

            using (var writer = new BinaryWriter(outputStream))
            {
                writer.Write(_graph.CurrentId);
                WriteIndices(writer, _graph);
                WriteVertexKeyIndices(writer, _graph);
                WriteEdgeKeyIndices(writer, _graph);
            }
        }

        /// <summary>
        ///     Write TinkerGraph metadata to an OutputStream.
        /// </summary>
        /// <param name="graph">the TinkerGraph to pull the metadata from</param>
        /// <param name="outputStream">the OutputStream to write the TinkerGraph metadata to</param>
        public static void Save(TinkerGraph graph, Stream outputStream)
        {
            Contract.Requires(graph != null);
            Contract.Requires(outputStream != null);

            var writer = new TinkerMetadataWriter(graph);
            writer.Save(outputStream);
        }

        /// <summary>
        ///     Write TinkerGraph metadata to a file.
        /// </summary>
        /// <param name="graph">the TinkerGraph to pull the data from</param>
        /// <param name="filename">the name of the file to write the TinkerGraph metadata to</param>
        public static void Save(TinkerGraph graph, string filename)
        {
            Contract.Requires(graph != null);
            Contract.Requires(!string.IsNullOrWhiteSpace(filename));

            var writer = new TinkerMetadataWriter(graph);
            writer.Save(filename);
        }

        private static void WriteIndices(BinaryWriter writer, TinkerGraph graph)
        {
            Contract.Requires(writer != null);
            Contract.Requires(graph != null);

            // Write the number of indices
            writer.Write(graph.Indices.Count);

            foreach (var index in graph.Indices)
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

        private static void WriteVertexKeyIndices(BinaryWriter writer, TinkerGraph graph)
        {
            Contract.Requires(writer != null);
            Contract.Requires(graph != null);

            // Write the number of vertex key indices
            writer.Write(graph.VertexKeyIndex.Index.Count);

            foreach (var index in graph.VertexKeyIndex.Index)
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

        private static void WriteEdgeKeyIndices(BinaryWriter writer, TinkerGraph graph)
        {
            Contract.Requires(writer != null);
            Contract.Requires(graph != null);

            // Write the number of edge key indices
            writer.Write(graph.EdgeKeyIndex.Index.Count);

            foreach (var index in graph.EdgeKeyIndex.Index)
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
            Contract.Requires(writer != null);
            Contract.Requires(data != null);

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