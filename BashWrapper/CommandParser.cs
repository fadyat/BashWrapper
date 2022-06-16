using System.Collections.Immutable;
using System.Text;
using System.Text.RegularExpressions;

namespace BashWrapper;

public static class CommandParser
{
    private static readonly SortedSet<string> MainCommands = new()
    {
        "pwd", "cat", "wc", "echo", "true", "false", "ls", "$?"
    };

    private static readonly SortedSet<string> Connectors = new()
    {
        "||", "&&", ";"
    };

    private const string AssignLocalVariablePattern = @"^\$\w+=.+";

    public static ImmutableList<string> Parse(string inputCommand)
    {
        var parsedCommand = new List<string>();
        var token = new StringBuilder();
        var inQuotas = false;

        foreach (var symbol in inputCommand)
        {
            switch (symbol)
            {
                case '"':
                    if (token.Length > 0)
                        parsedCommand.Add(token.ToString());
                    token.Clear();
                    inQuotas = !inQuotas;
                    break;
                case ' ' when !inQuotas:
                    if (token.Length > 0) parsedCommand.Add(token.ToString());
                    token.Clear();
                    break;
                default:
                    token.Append(symbol);
                    break;
            }
        }

        if (token.Length > 0)
        {
            parsedCommand.Add(token.ToString());
        }

        return parsedCommand.ToImmutableList();
    }

    public static ImmutableList<ImmutableList<string>> SplitArgsByConnectors(ImmutableList<string> args)
    {
        var allCommands = new List<ImmutableList<string>>();
        var currentCommand = new List<string>();

        var i = 0;
        while (i < args.Count)
        {
            while (i < args.Count && !Connectors.Contains(args[i]))
                currentCommand.Add(args[i++]);

            allCommands.Add(currentCommand.ToImmutableList());
            currentCommand.Clear();

            if (i < args.Count) allCommands.Add(new List<string> {args[i++]}.ToImmutableList());
        }
        
        if (allCommands.Any(command =>
                !MainCommands.Contains(command.First()) &&
                !Regex.IsMatch(command.First(), AssignLocalVariablePattern) &&
                !Connectors.Contains(command.First())))
        {
            throw new ArgumentException("Incorrect command!");
        }

        return allCommands.ToImmutableList();
    }

    public static (string, ImmutableList<string>) ParseCommandArgs(ImmutableList<string> command)
    {
        var mainArg = command.First();
        var commandArgs = command.GetRange(1, command.Count - 1);
        return (mainArg, commandArgs);
    }

    public static ImmutableList<string> ReplaceLocalVariables(ImmutableList<string> commandArgs,
        Dictionary<string, string> localVariablesValues)
    {
        var args = commandArgs.ToList();
        for (var i = 0; i < args.Count; i++)
        {
            var command = args[i];
            if (command.First() != '$' || command.Contains('=')) continue;
            var variable = command[1..];
            args[i] = localVariablesValues[variable];
        }

        return args.ToImmutableList();
    }
}