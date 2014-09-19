using System;

namespace ConsoleApplication
{
    public static class ArgumentMarshalerExtensions
    {
        public static T GetValue<T>(this IArgumentMarshaler @this, T defaultValue)
        {
            try
            {
                return @this != null ? (T)@this.Get() : defaultValue;
            }
            catch (InvalidCastException)
            {
                throw new ArgsException();
            }
        }
    }
}