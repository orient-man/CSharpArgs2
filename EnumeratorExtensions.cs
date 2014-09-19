using System.Collections.Generic;

namespace ConsoleApplication
{
    public static class EnumeratorExtensions
    {
        public static T Next<T>(this IEnumerator<T> enumerator)
        {
            enumerator.MoveNext();
            return enumerator.Current;
        }
    }
}