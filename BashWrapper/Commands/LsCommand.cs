using System.Collections.Immutable;

namespace BashWrapper.Commands;

public class LsCommand : AbstractCommand
{
    private readonly string _currentDirectory;

    public LsCommand(ImmutableList<string> args) : base(args)
    {
        var pwdCommand = new PwdCommand(ImmutableList<string>.Empty);
        _currentDirectory = (pwdCommand.Execute() as string)!;
    }

    public override object Execute()
    {
        if (!CanExecute())
            throw new DirectoryNotFoundException("Some of the directories doesn't exists!");

        if (!Args.Any())
        {
            var directoryContent = Directory.GetFileSystemEntries(_currentDirectory);
            return new List<ImmutableList<string>> {directoryContent.ToImmutableList()}.ToImmutableList();
        }

        return Args.Select(path =>
        {
            var directoryPath = Path.Combine(_currentDirectory, path);
            var directoryContent = Directory.GetFileSystemEntries(directoryPath);
            return directoryContent.Select(Path.GetFullPath).ToImmutableList();
        }).ToImmutableList();
    }

    protected override bool CanExecute()
    {
        return Args.All(path =>
        {
            var directoryPath = Path.Combine(_currentDirectory, path);
            return Directory.Exists(directoryPath);
        });
    }
}