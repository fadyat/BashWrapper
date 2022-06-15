using System.Collections.Immutable;

namespace BashWrapper.Commands;

public class CatCommand : AbstractCommand
{
    private readonly string _currentDirectory;

    public CatCommand(ImmutableList<string> args) : base(args)
    {
        var pwdCommand = new PwdCommand(ImmutableList<string>.Empty);
        _currentDirectory = (pwdCommand.Execute() as string)!;
    }

    public override object Execute()
    {
        if (!CanExecute())
            throw new FileNotFoundException("Some of the files doesn't exists!");

        return Args.Select(path =>
        {
            var filePath = Path.Combine(_currentDirectory, path);
            var fileContent = File.ReadLines(filePath);
            return fileContent.ToImmutableList();
        }).ToImmutableList();
    }

    protected override bool CanExecute()
    {
        return Args.All(path =>
        {
            var filePath = Path.Combine(_currentDirectory, path);
            return File.Exists(filePath);
        });
    }
}