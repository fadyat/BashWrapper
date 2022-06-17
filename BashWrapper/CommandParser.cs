using System.Collections.Immutable;
using System.Text;
using System.Text.RegularExpressions;

namespace BashWrapper;

public static class CommandParser
{
    private static readonly SortedSet<string> MainCommands = new()
    {
        "pwd", "cat", "wc", "echo", "true", "false", "ls", "exit"
    };

    private static readonly SortedSet<string> Connectors = new()
    {
        "||", "&&", ";"
    };

    private static readonly SortedSet<string> Redirectors = new()
    {
        ">", ">>", "<"
    };

    private const string AssignLocalVariablePattern = @"^\$\w+=.+";
    private const string LocalVariablePattern = @"^\$\w+";

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

        if (token.Length > 0) parsedCommand.Add(token.ToString());

        return parsedCommand.ToImmutableList();
    }

    public static ImmutableList<ImmutableList<string>> SplitArgsByConnectorsAndRedirectors(ImmutableList<string> args)
    {
        var allCommands = new List<ImmutableList<string>>();
        var currentCommand = new List<string>();

        var i = 0;
        while (i < args.Count)
        {
            while (i < args.Count && !Connectors.Contains(args[i]) && !Redirectors.Contains(args[i]))
                currentCommand.Add(args[i++]);

            if (currentCommand.Any())
            {
                allCommands.Add(currentCommand.ToImmutableList());
                currentCommand.Clear();
            }

            if (i >= args.Count) continue;

            if (IsConnector(args[i]))
                allCommands.Add(new List<string> {args[i++]}.ToImmutableList());
            else if (IsRedirector(args[i]))
                allCommands.Add(new List<string> {args[i++], args[i++]}.ToImmutableList());
        }

        if (allCommands.Any(command =>
                !IsMainCommand(command.First()) &&
                !Regex.IsMatch(command.First(), AssignLocalVariablePattern) &&
                !IsConnector(command.First()) &&
                !IsRedirector(command.First())))
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
            var variable = args[i];
            if (variable.First() != '$' || variable.Contains('=')) continue;
            args[i] = localVariablesValues.ContainsKey(variable) ? localVariablesValues[variable] : string.Empty;
        }

        return args.ToImmutableList();
    }

    public static bool IsMainCommand(string mainArg)
    {
        return MainCommands.Contains(mainArg);
    }

    public static bool IsConnector(string mainArg)
    {
        return Connectors.Contains(mainArg);
    }

    public static bool IsRedirector(string mainArg)
    {
        return Redirectors.Contains(mainArg);
    }

    public static bool IsVariable(string mainArg)
    {
        return Regex.IsMatch(mainArg, LocalVariablePattern);
    }
}