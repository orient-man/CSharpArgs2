namespace ConsoleApplication
{
    public static class ArgumentMarshalerExtensions
    {
        public static T GetValue<T>(this IArgumentMarshaler @this, T defaultValue)
        {
            if (@this == null)
                return defaultValue;

            var value = @this.Get();
            return value is T ? (T)value : defaultValue;
        }
    }
}