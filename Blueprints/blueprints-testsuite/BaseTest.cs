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
        Stopwatch _Timer = null;

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

        public static int Count<T>(CloseableIterable<T> iterable)
        {
            return Count(iterable.GetEnumerator());
        }

        public static List<T> AsList<T>(IEnumerable<T> iterable)
        {
            List<T> list = new List<T>();
            foreach (T object_ in iterable)
                list.Add(object_);
            
            return list;
        }

        public long StopWatch()
        {
            if (_Timer == null)
            {
                _Timer = Stopwatch.StartNew();
                return -1;
            }
            else
                return _Timer.ElapsedMilliseconds;
        }

        public static void PrintPerformance(string name, object events, string eventName, long timeInMilliseconds)
        {
            if (null != events)
                Console.WriteLine(string.Concat("\t", name, ": ", (int)events, " ", eventName, " in ", timeInMilliseconds, "ms"));
            else
                Console.WriteLine(string.Concat("\t", name, ": ", eventName, " in ", timeInMilliseconds, "ms"));
        }

        public static void PrintTestPerformance(string testName, long timeInMilliseconds)
        {
            Console.WriteLine(string.Concat("*** TOTAL TIME [", testName, "]: ", timeInMilliseconds, " ***"));
        }

        protected static void DeleteDirectory(string directory)
        {
            if (Directory.Exists(directory))
                Directory.Delete(directory, true);
        }

        public string ComputeTestDataRoot()
        {
            return this.GetType().Namespace.Replace('.', '\\');
        }
    }
}
