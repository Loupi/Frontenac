using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Frontenac.Blueprints
{
    public abstract class BaseTest
    {
        Stopwatch _timer = null;

        public static T getOnlyElement<T>(IEnumerator<T> iterator)
        {
            if (!iterator.MoveNext()) return default(T);
            T element = iterator.Current;
            if (iterator.MoveNext()) throw new ArgumentException("Iterator has multiple elmenets");
            return element;
        }

        public static T getOnlyElement<T>(IEnumerable<T> iterable)
        {
            return getOnlyElement(iterable.GetEnumerator());
        }

        public static int count(IEnumerator iterator)
        {
            int counter = 0;
            while (iterator.MoveNext())
                counter++;
            
            return counter;
        }

        public static int count(IEnumerable iterable)
        {
            return count(iterable.GetEnumerator());
        }

        public static int count<T>(CloseableIterable<T> iterable)
        {
            return count(iterable.GetEnumerator());
        }

        public static List<T> asList<T>(IEnumerable<T> iterable)
        {
            List<T> list = new List<T>();
            foreach (T object_ in iterable)
                list.Add(object_);
            
            return list;
        }

        public long stopWatch()
        {
            if (_timer == null)
            {
                _timer = Stopwatch.StartNew();
                return -1;
            }
            else
                return _timer.ElapsedMilliseconds;
        }

        public static void printPerformance(string name, object events, string eventName, long timeInMilliseconds)
        {
            if (null != events)
                Console.WriteLine(string.Concat("\t", name, ": ", (int)events, " ", eventName, " in ", timeInMilliseconds, "ms"));
            else
                Console.WriteLine(string.Concat("\t", name, ": ", eventName, " in ", timeInMilliseconds, "ms"));
        }

        public static void printTestPerformance(string testName, long timeInMilliseconds)
        {
            Console.WriteLine(string.Concat("*** TOTAL TIME [", testName, "]: ", timeInMilliseconds, " ***"));
        }

        protected static void deleteDirectory(string directory)
        {
            if (Directory.Exists(directory))
                Directory.Delete(directory, true);

            // overkill code, simply allowing us to detect when data dir is in use. useful though because without it
            // tests may fail if a database is re-used in between tests somehow. this directory really needs to be
            // cleared between tests runs and this exception will make it clear if it is not.
            if (Directory.Exists(directory))
                throw new Exception(string.Concat("unable to delete directory ", Path.GetFullPath(directory)));
        }

        public string computeTestDataRoot()
        {
            return this.GetType().Namespace.Replace('.', '\\');
        }
    }
}
