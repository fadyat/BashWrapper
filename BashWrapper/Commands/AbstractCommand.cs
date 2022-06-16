using System.Collections.Immutable;
using System.Text;

namespace BashWrapper.Commands;

public abstract class AbstractCommand
{
    public ImmutableList<string> Args { get; private set; }
    protected StringBuilder Buffer { get; }

    protected AbstractCommand(ImmutableList<string> args, StringBuilder buffer)
    {
        Args = args;
        Buffer = buffer;
    }

    public void AddArgument(string arg)
    {
        Args = Args.Add(arg);
    }

    public abstract object Execute();

    protected abstract bool CanExecute();

    public abstract StringBuilder ToBuffer();
}