using System.Collections.Immutable;
using System.Text;

namespace BashWrapper.Commands.RedirectCommands;

public class InputRedirectCommand : AbstractCommand
{
    private readonly string _currentDirectory;
    private readonly bool _overwrite;

    public InputRedirectCommand(ImmutableList<string> args, StringBuilder buffer, bool overwrite = false) : base(args, buffer)
    {
        var pwdCommand = new PwdCommand(ImmutableList<string>.Empty, buffer);
        _currentDirectory = (pwdCommand.Execute() as string)!;
        _overwrite = overwrite;
    }

    public override object Execute()
    {
        if (!CanExecute())
            throw new ArgumentException($"Wrong args number {Args.Count}, expected 1!");

        var filePath = Path.GetFullPath(Path.Combine(_currentDirectory, Args[0]));
        if (_overwrite) File.Delete(filePath);
        if (!File.Exists(filePath)) using (File.Create(filePath)) { }

        File.AppendAllText(filePath, Buffer.ToString());
        return filePath;
    }

    protected override bool CanExecute()
    {
        return Args.Count == 1;
    }

    public override StringBuilder ToBuffer()
    {
        return new StringBuilder();
    }
}