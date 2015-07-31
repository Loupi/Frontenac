using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using Frontenac.Blueprints.Impls.TG;
using Frontenac.Blueprints.Util.IO.GML;
using Frontenac.Blueprints.Util.IO.GraphML;
using Frontenac.Blueprints.Util.IO.GraphSON;

namespace Frontenac.Blueprints
{
    public static class GraphHelpers
    {
        private const string GraphFileDotNet = "/tinkergraph.dat";
        private const string GraphFileGml = "/Gravegraph.gml";
        private const string GraphFileGraphson = "/Gravegraph.json";
        private const string GraphFileGraphml = "/Gravegraph.xml";

        public const string ContractExceptionName = "System.Diagnostics.Contracts.__ContractsRuntime+ContractException";

        public static TValue Get<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key)
        {
            Contract.Requires(dictionary != null);

            TValue ret;
            dictionary.TryGetValue(key, out ret);
            return ret;
        }

        public static TValue Get<TValue>(this IList<TValue> list, int at)
        {
            Contract.Requires(list != null);

            var ret = list.ElementAtOrDefault(at);
            return ret;
        }

        public static TValue JavaRemove<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key)
        {
            var ret = default(TValue);
            if (!Equals(key, default(TKey)))
            {
                if (dictionary.TryGetValue(key, out ret))
                    dictionary.Remove(key);
            }
            return ret;
        }

        public static TValue Put<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key, TValue value)
        {
            Contract.Requires(dictionary != null);

            TValue ret;
            if (dictionary.TryGetValue(key, out ret))
                dictionary[key] = value;
            else
                dictionary.Add(key, value);
            return ret;
        }

        [Pure]
        public static bool IsNumber(object expression)
        {
            return expression is byte ||
                   expression is sbyte ||
                   expression is ushort ||
                   expression is short ||
                   expression is uint ||
                   expression is int ||
                   expression is ulong ||
                   expression is long ||
                   expression is float ||
                   expression is double ||
                   expression is decimal;
        }

        public static IGraph LoadDotNet(string directory)
        {
            using (var stream = File.OpenRead(string.Concat(directory, GraphFileDotNet)))
            {
                var formatter = new BinaryFormatter();
                return (IGraph)formatter.Deserialize(stream);
            }
        }

        public static void SaveDotNet(this IGraph graph, string directory)
        {
            Contract.Requires(graph != null);

            var filePath = string.Concat(directory, GraphFileDotNet);
            DeleteFile(filePath);
            using (var stream = File.Create(string.Concat(directory, GraphFileDotNet)))
            {
                var formatter = new BinaryFormatter();
                formatter.Serialize(stream, graph);
            }
        }

        static void DeleteFile(string path)
        {
            if (File.Exists(path))
                File.Delete(path);
        }

        public static void LoadGml(this IGraph graph, string directory)
        {
            Contract.Requires(graph != null);

            GmlReader.InputGraph(graph, string.Concat(directory, GraphFileGml));
        }

        public static void SaveGml(this IGraph graph, string directory)
        {
            Contract.Requires(graph != null);

            var filePath = string.Concat(directory, GraphFileGml);
            DeleteFile(filePath);
            GmlWriter.OutputGraph(graph, filePath);
        }

        public static void LoadGraphml(this IGraph graph, string directory)
        {
            Contract.Requires(graph != null);

            GraphMlReader.InputGraph(graph, string.Concat(directory, GraphFileGraphml));
        }

        public static void SaveGraphml(this IGraph graph, string directory)
        {
            Contract.Requires(graph != null);

            var filePath = string.Concat(directory, GraphFileGraphml);
            DeleteFile(filePath);
            GraphMlWriter.OutputGraph(graph, filePath);
        }

        public static void LoadGraphson(this IGraph graph, string directory)
        {
            Contract.Requires(graph != null);

            GraphSonReader.InputGraph(graph, string.Concat(directory, GraphFileGraphson));
        }

        public static void SaveGraphson(this IGraph graph, string directory)
        {
            Contract.Requires(graph != null);

            var filePath = string.Concat(directory, GraphFileGraphson);
            DeleteFile(filePath);
            GraphSonWriter.OutputGraph(graph, filePath, GraphSonMode.EXTENDED);
        }

        public static void CreateTinkerGraph(this IGraph graph)
        {
            Contract.Requires(graph != null);

            TinkerGraphFactory.CreateTinkerGraph(graph);
        }
    }
}