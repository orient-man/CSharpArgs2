using System;
using System.Collections.Generic;
using System.Linq;

namespace ConsoleApplication
{
    public class Args
    {
        private static readonly IDictionary<string, Func<IArgumentMarshaler>> Marshalers =
            new Dictionary<string, Func<IArgumentMarshaler>>
            {
                { "", () => new BoolArgumentMarshaler() },
                { "*", () => new StringArgumentMarshaler() },
                { "#", () => new IntArgumentMarshaler() },
                { "##", () => new DoubleArgumentMarshaler() }
            };

        private readonly IDictionary<char, IArgumentMarshaler> argsFound;

        public Args(string schema, IEnumerable<string> args)
        {
            argsFound =
                ParseArguments(args, ParseSchema(schema))
                    .ToDictionary(o => o.Item1, o => o.Item2);
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
            try
            {
                return Marshalers[elementTail]();
            }
            catch (KeyNotFoundException)
            {
                throw new ArgsException(
                    ErrorCode.InvalidArgumentFormat,
                    elementId,
                    elementTail);
            }
        }

        private static void ValidateSchemaElementId(char elementId)
        {
            if (!char.IsLetter(elementId))
                throw new ArgsException(ErrorCode.InvalidArgumentName, elementId);
        }

        private static IEnumerable<Tuple<char, IArgumentMarshaler>> ParseArguments(
            IEnumerable<string> args,
            IDictionary<char, IArgumentMarshaler> marshalers)
        {
            var currentArgument = args.GetEnumerator();
            while (currentArgument.MoveNext())
                foreach (var arg in FindElements(currentArgument.Current))
                {
                    IArgumentMarshaler m;
                    try
                    {
                        m = marshalers[arg];
                        m.Set(currentArgument);
                    }
                    catch (KeyNotFoundException)
                    {
                        throw new ArgsException(ErrorCode.UnexpectedArgument, arg);
                    }
                    catch (ArgsException e)
                    {
                        e.ErrorArgumentId = arg;
                        throw;
                    }

                    yield return new Tuple<char, IArgumentMarshaler>(arg, m);
                }
        }

        private static IEnumerable<char> FindElements(string arg)
        {
            return arg.StartsWith("-") ? arg.Skip(1) : Enumerable.Empty<char>();
        }

        public int Cardinality()
        {
            return argsFound.Count;
        }

        public string GetString(char arg)
        {
            return argsFound[arg].GetValueOrDefault("");
        }

        public int GetInt(char arg)
        {
            return argsFound[arg].GetValueOrDefault(0);
        }

        public bool GetBoolean(char arg)
        {
            return argsFound[arg].GetValueOrDefault(false);
        }

        public double GetDouble(char arg)
        {
            return argsFound[arg].GetValueOrDefault(0.0);
        }

        public bool Has(char arg)
        {
            return argsFound.ContainsKey(arg);
        }
    }
}