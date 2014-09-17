using System;

namespace ConsoleApplication
{
    public interface IArgumentMarshaler
    {
        void Set(Iterator<String> currentArgument);
    }
}