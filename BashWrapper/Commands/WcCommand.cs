using System.Collections.Immutable;
using System.Diagnostics.Tracing;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.CodeAnalysis;

namespace BashWrapper.Commands;

public class WcCommand : AbstractCommand
{
    private readonly string _currentDirectory;

    public WcCommand(ImmutableList<string> args) : base(args)
    {
        var pwdCommand = new PwdCommand(ImmutableList<string>.Empty);
        _currentDirectory = (pwdCommand.Execute() as string)!;
    }

    public override object Execute()
    {
        if (!CanExecute())
            throw new ArgumentException("Not enough args!");
        
        return Args.Select(arg =>
        {
            var filePath = Path.GetFullPath(Path.Combine(_currentDirectory, arg));
            var (lines, words) = (0, 0);
            foreach (var line in File.ReadLines(filePath))
            {
                lines++;
                words += Regex.Replace(line, "{2, }", " ").Trim().Split(' ').Length;
            }

            var bytes = File.ReadAllBytes(filePath).Length;
            return (lines, words, bytes);
        }).ToImmutableList();
    }

    protected override bool CanExecute()
    {
        return Args.Any();
    }
}