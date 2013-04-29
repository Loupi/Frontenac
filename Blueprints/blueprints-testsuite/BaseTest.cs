using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace Frontenac.Blueprints
{
    public abstract class BaseTest
    {
        Stopwatch _timer;

        public static T GetOnlyElement<T>(IEnumerator<T> iterator)
        {
            if (!iterator.MoveNext()) return default(T);
            T element = iterator.Current;
            if (iterator.MoveNext()) throw new ArgumentException("Iterator has multiple elmenets");
            return element;
        }

        public static T GetOnlyElement<T>(IEnumerable<T> iterable)
        {
            return GetOnlyElement(iterable.GetEnumerator());
        }

        public static int Count(IEnumerator iterator)
        {
            int counter = 0;
            while (iterator.MoveNext())
                counter++;
            
            return counter;
        }

        public static int Count(IEnumerable iterable)
        {
            return Count(iterable.GetEnumerator());
        }

        public static int Count<T>(ICloseableIterable<T> iterable)
        {
            return Count(iterable.GetEnumerator());
        }

        public static List<T> AsList<T>(IEnumerable<T> iterable)
        {
            return iterable.ToList();
        }

        public long StopWatch()
        {
            if (_timer == null)
            {
                _timer = Stopwatch.StartNew();
                return -1;
            }
            return _timer.ElapsedMilliseconds;
        }

        public static void PrintPerformance(string name, object events, string eventName, long timeInMilliseconds)
        {
            Console.WriteLine(null != events
                                  ? string.Concat("\t", name, ": ", (int) events, " ", eventName, " in ",
                                                  timeInMilliseconds, "ms")
                                  : string.Concat("\t", name, ": ", eventName, " in ", timeInMilliseconds, "ms"));
        }

        public static void PrintTestPerformance(string testName, long timeInMilliseconds)
        {
            Console.WriteLine(string.Concat("*** TOTAL TIME [", testName, "]: ", timeInMilliseconds, " ***"));
        }

        protected static void DeleteDirectory(string directory)
        {
            if (Directory.Exists(directory))
                Directory.Delete(directory, true);

            // overkill code, simply allowing us to detect when data dir is in use. useful though because without it
            // tests may fail if a database is re-used in between tests somehow. this directory really needs to be
            // cleared between tests runs and this exception will make it clear if it is not.
            if (Directory.Exists(directory))
                throw new Exception(string.Concat("unable to delete directory ", Path.GetFullPath(directory)));
        }

        public string ComputeTestDataRoot()
        {
            var ns = GetType().Namespace;
            if (ns != null) return ns.Replace('.', '\\');
            return null;
        }
    }
}
