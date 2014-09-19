using System.Collections.Generic;

namespace ConsoleApplication
{
    public interface IArgumentMarshaler<out T> : IArgumentMarshaler
    {
        T Value { get; }
    }

    public interface IArgumentMarshaler
    {
        void Set(IEnumerator<string> currentArgument);
    }
}