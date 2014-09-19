namespace ConsoleApplication
{
    public static class ArgumentMarshalerExtensions
    {
        public static T GetValueOrDefault<T>(this IArgumentMarshaler @this, T defaultValue)
        {
            var m = @this as IArgumentMarshaler<T>;
            return m == null ? defaultValue : m.Value;
        }
    }
}