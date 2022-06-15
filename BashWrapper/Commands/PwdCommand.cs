using System.Collections.Immutable;

namespace BashWrapper.Commands;

public class PwdCommand : AbstractCommand
{
    public PwdCommand(ImmutableList<string> args) : base(args)
    {
    }

    public override object Execute()
    {
        if (!CanExecute())
            throw new ArgumentException($"Too much args {Args.Count} expected 0!");

        var currentDirectory = Directory.GetCurrentDirectory();
        return currentDirectory;
    }

    protected override bool CanExecute()
    {
        return !Args.Any();
    }
}