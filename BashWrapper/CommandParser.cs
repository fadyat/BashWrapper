using System.Collections.Immutable;
using System.Text;

namespace BashWrapper;

public static class CommandParser
{
    private static readonly SortedSet<string> MainCommands = new()
    {
        "pwd", "cat", "wc", "echo", "true", "false", "||", "&&", ";", "ls"
    };

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

    public static ImmutableList<ImmutableList<string>> SplitArgsByMainCommands(ImmutableList<string> args)
    {
        var allCorrectCommands = new List<ImmutableList<string>>();
        var currentCommand = new List<string>();

        if (!MainCommands.Contains(args.First()))
            throw new ArgumentException("Incorrect command!");

        var i = 0;
        while (i < args.Count)
        {
            currentCommand.Add(args[i++]);
            while (i < args.Count && !MainCommands.Contains(args[i]))
                currentCommand.Add(args[i++]);

            allCorrectCommands.Add(currentCommand.ToImmutableList());
            currentCommand.Clear();
        }

        return allCorrectCommands.ToImmutableList();
    }
}