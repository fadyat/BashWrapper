using System.Collections.Immutable;

namespace BashWrapper.Commands;

public abstract class AbstractCommand
{
    protected ImmutableList<string> Args { get; }

    protected AbstractCommand(ImmutableList<string> args)
    {
        Args = args;
    }

    public abstract object Execute();

    protected abstract bool CanExecute();
}