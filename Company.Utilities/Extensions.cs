using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Company.Utilities
{
    public static class Extensions
    {
        public static T Clone<T>(this T source)
        {
            if (source == null)
            {
                return default(T);
            }

            T val = JsonSerializer.Deserialize<T>(JsonSerializer.Serialize(source));
            if (val == null)
            {
                return default(T);
            }

            return val;
        }

        public static IEnumerable<IEnumerable<T>> Batch<T>(this IEnumerable<T> source, int size)
        {
            T[] array = null;
            int num = 0;
            foreach (T item in source)
            {
                if (array == null)
                {
                    array = new T[size];
                }

                array[num++] = item;
                if (num == size)
                {
                    yield return array.Select((T x) => x);
                    array = null;
                    num = 0;
                }
            }

            if (array != null && num > 0)
            {
                yield return array.Take(num);
            }
        }
    }
}
