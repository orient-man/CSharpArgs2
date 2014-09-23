using System;
using System.Collections.Generic;
using System.Linq;
using ArgsDictionary =
    System.Collections.Generic.Dictionary<char, ConsoleApplication.IArgumentMarshaler>;

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

        private readonly ArgsDictionary argsFound;

        public Args(string schema, IEnumerable<string> args)
        {
            argsFound = ParseArguments(args.GetEnumerator(), ParseSchema(schema));
        }

        private static ArgsDictionary ParseSchema(string schema)
        {
            return schema
                .Split(',')
                .Select(o => o.Trim())
                .Where(o => o.Length > 0)
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

        private static ArgsDictionary ParseArguments(
            IEnumerator<string> currentArgument,
            ArgsDictionary marshalers)
        {
            var argsFound = new ArgsDictionary();
            while (currentArgument.MoveNext())
                foreach (var arg in FindArguments(currentArgument.Current))
                {
                    try
                    {
                        var m = marshalers[arg];
                        argsFound[arg] = m;
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
                }

            return argsFound;
        }

        private static IEnumerable<char> FindArguments(string arg)
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