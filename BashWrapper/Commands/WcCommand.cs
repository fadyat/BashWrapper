using System.Collections.Immutable;
using System.Text;
using System.Text.RegularExpressions;

namespace BashWrapper.Commands;

public class WcCommand : AbstractCommand
{
    private readonly string _currentDirectory;
    private ImmutableList<(int lines, int words, int bytes)> _result;

    public WcCommand(ImmutableList<string> args, StringBuilder buffer) : base(args, buffer)
    {
        var pwdCommand = new PwdCommand(ImmutableList<string>.Empty, null!);
        _currentDirectory = (pwdCommand.Execute() as string)!;
        _result = ImmutableList<(int lines, int words, int bytes)>.Empty;
    }

    public override object Execute()
    {
        if (!CanExecute())
            throw new ArgumentException("Not enough args!");

        _result = Args.Select(arg =>
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

        return _result;
    }

    protected override bool CanExecute()
    {
        return Args.Any();
    }

    public override StringBuilder ToBuffer()
    {
        _result.ForEach(x => Buffer.AppendLine($"{x.lines} |\t{x.words} |\t{x.bytes}"));
        return Buffer;
    }
}