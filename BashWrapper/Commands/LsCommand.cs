using System.Collections.Immutable;
using System.Text;

namespace BashWrapper.Commands;

public class LsCommand : AbstractCommand
{
    private readonly string _currentDirectory;
    private ImmutableList<ImmutableList<string>> _result;

    public LsCommand(ImmutableList<string> args, StringBuilder buffer) : base(args, buffer)
    {
        var pwdCommand = new PwdCommand(ImmutableList<string>.Empty, null!);
        _currentDirectory = (pwdCommand.Execute() as string)!;
        _result = ImmutableList<ImmutableList<string>>.Empty;
    }

    public override object Execute()
    {
        if (!CanExecute())
            throw new DirectoryNotFoundException("Some of the directories doesn't exists!");

        if (!Args.Any())
        {
            var directoryContent = Directory.GetFileSystemEntries(_currentDirectory);
            _result = new List<ImmutableList<string>> {directoryContent.ToImmutableList()}.ToImmutableList();
        }
        else
        {
            _result = Args.Select(path =>
            {
                var directoryPath = Path.Combine(_currentDirectory, path);
                var directoryContent = Directory.GetFileSystemEntries(directoryPath);
                return directoryContent.Select(Path.GetFullPath).ToImmutableList();
            }).ToImmutableList();
        }

        return _result;
    }

    protected override bool CanExecute()
    {
        return Args.All(path =>
        {
            var directoryPath = Path.Combine(_currentDirectory, path);
            return Directory.Exists(directoryPath);
        });
    }

    public override StringBuilder ToBuffer()
    {
        _result.ForEach(x => x.ForEach(y => Buffer.AppendLine(y)));
        return Buffer;
    }
}