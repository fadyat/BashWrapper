using System.Collections.Immutable;

namespace BashWrapper.Commands;

// works like simple system.output; don't work with regex
public class EchoCommand : AbstractCommand
{
    public EchoCommand(ImmutableList<string> args) : base(args)
    {
    }

    public override object Execute()
    {
        if (!CanExecute()) return 0; // future checks may written here
        return string.Join(" ", Args);
    }

    protected override bool CanExecute()
    {
        return true;
    }
}