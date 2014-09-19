using System;
using System.Collections.Generic;
using System.Linq;

namespace ConsoleApplication
{
    public class Args
    {
        private static readonly IDictionary<string, Func<IArgumentMarshaler>> AvailableMarshalers =
            new Dictionary<string, Func<IArgumentMarshaler>>
            {
                { "", () => new BoolArgumentMarshaler() },
                { "*", () => new StringArgumentMarshaler() },
                { "#", () => new IntArgumentMarshaler() },
                { "##", () => new DoubleArgumentMarshaler() }
            };

        private readonly IEnumerator<string> currentArgument;
        private readonly IDictionary<char, IArgumentMarshaler> marshalers;
        private readonly ISet<char> argsFound;

        public Args(string schema, IEnumerable<string> args)
        {
            currentArgument = args.GetEnumerator();
            marshalers = ParseSchema(schema);
            argsFound = new HashSet<char>(ParseArguments());
        }

        private static IDictionary<char, IArgumentMarshaler> ParseSchema(string schema)
        {
            return schema
                .Split(',')
                .Where(o => o.Length > 0)
                .Select(o => o.Trim())
                .Select(o => new { ElementId = o[0], ElementTail = o.Substring(1) })
                .ToDictionary(
                    o => o.ElementId,
                    o => ParseSchemaElement(o.ElementId, o.ElementTail));
        }

        private static IArgumentMarshaler ParseSchemaElement(
            char elementId,
            string elementTail)
        {
            ValidateSchemaElementId(elementId);
            Func<IArgumentMarshaler> marshalerFactory;
            if (!AvailableMarshalers.TryGetValue(elementTail, out marshalerFactory))
                throw new ArgsException(
                    ErrorCode.InvalidArgumentFormat,
                    elementId,
                    elementTail);

            return marshalerFactory();
        }

        private static void ValidateSchemaElementId(char elementId)
        {
            if (!char.IsLetter(elementId))
                throw new ArgsException(ErrorCode.InvalidArgumentName, elementId);
        }

        private IEnumerable<char> ParseArguments()
        {
            while (currentArgument.MoveNext())
                foreach (var arg in ParseArgument(currentArgument.Current))
                    yield return arg;
        }

        private IEnumerable<char> ParseArgument(string arg)
        {
            return arg.StartsWith("-") ? ParseElements(arg) : Enumerable.Empty<char>();
        }

        private IEnumerable<char> ParseElements(string arg)
        {
            return arg.Skip(1).Select(ParseElement);
        }

        private char ParseElement(char argChar)
        {
            if (SetArgument(argChar))
                return argChar;

            throw new ArgsException(ErrorCode.UnexpectedArgument, argChar);
        }

        private bool SetArgument(char argChar)
        {
            var m = GetMarshaler(argChar);
            if (m == null)
                return false;

            try
            {
                m.Set(currentArgument);
            }
            catch (ArgsException e)
            {
                e.ErrorArgumentId = argChar;
                throw;
            }

            return true;
        }

        public int Cardinality()
        {
            return argsFound.Count;
        }

        public string GetString(char arg)
        {
            return GetMarshaler(arg).GetValueOrDefault("");
        }

        public int GetInt(char arg)
        {
            return GetMarshaler(arg).GetValueOrDefault(0);
        }

        public bool GetBoolean(char arg)
        {
            return GetMarshaler(arg).GetValueOrDefault(false);
        }

        public double GetDouble(char arg)
        {
            return GetMarshaler(arg).GetValueOrDefault(0.0);
        }

        private IArgumentMarshaler GetMarshaler(char arg)
        {
            IArgumentMarshaler m;
            return !marshalers.TryGetValue(arg, out m) ? null : m;
        }

        public bool Has(char arg)
        {
            return argsFound.Contains(arg);
        }
    }
}