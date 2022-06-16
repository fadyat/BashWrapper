using System.Collections.Immutable;
using System.Text;

namespace BashWrapper.Commands;

public class CatCommand : AbstractCommand
{
    private readonly string _currentDirectory;
    private ImmutableList<ImmutableList<string>> _result;

    public CatCommand(ImmutableList<string> args, StringBuilder buffer) : base(args, buffer)
    {
        var pwdCommand = new PwdCommand(ImmutableList<string>.Empty, null!);
        _currentDirectory = (pwdCommand.Execute() as string)!;
        _result = ImmutableList<ImmutableList<string>>.Empty;
    }

    public override object Execute()
    {
        if (!CanExecute())
            throw new FileNotFoundException("Some of the files doesn't exists!");

        _result = Args.Select(path =>
        {
            var filePath = Path.Combine(_currentDirectory, path);
            var fileContent = File.ReadLines(filePath);
            return fileContent.ToImmutableList();
        }).ToImmutableList();

        return _result;
    }

    protected override bool CanExecute()
    {
        return Args.All(path =>
        {
            var filePath = Path.Combine(_currentDirectory, path);
            return File.Exists(filePath);
        });
    }

    public override StringBuilder ToBuffer()
    {
        _result.ForEach(x => x.ForEach(y => Buffer.AppendLine(y)));
        return Buffer;
    }
}