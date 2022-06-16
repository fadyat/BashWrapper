using System.Collections.Immutable;

namespace BashWrapper.Commands;

public class ExitCommand : AbstractCommand
{
    public ExitCommand(ImmutableList<string> args) : base(args)
    {
    }

    public override object Execute()
    {
        if (!CanExecute())
        {
            if (Args.Count != 1) throw new ArgumentException($"Wrong args number {Args.Count}, expected 1!");
            throw new ArgumentException("Exit code must be integer!");
        }

        var exitCode = Args.Any() ? Args[0] : "0";
        return exitCode;
    }

    protected override bool CanExecute()
    {
        return Args.Count == 1 && int.TryParse(Args[0], out _) || !Args.Any();
    }
}