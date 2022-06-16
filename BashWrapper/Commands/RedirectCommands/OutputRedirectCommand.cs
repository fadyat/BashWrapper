using System.Collections.Immutable;
using System.Text;

namespace BashWrapper.Commands.RedirectCommands;

public class OutputRedirectCommand : AbstractCommand
{
    private readonly Stack<AbstractCommand> _commandStack;
    private AbstractCommand _topCommand;

    public OutputRedirectCommand(ImmutableList<string> args, StringBuilder buffer, Stack<AbstractCommand> commandStack)
        : base(args, buffer)
    {
        _commandStack = commandStack;
        _topCommand = null!;
    }

    public override object Execute()
    {
        if (!CanExecute())
        {
            if (Args.Count != 1) throw new ArgumentException($"Wrong args number {Args.Count}, expected 1!");
            throw new ArgumentException("Can't redirect input to void!");
        }

        _topCommand = _commandStack.Pop();
        _topCommand.AddArgument(Args[0]);
        return _topCommand.Execute();
    }

    protected override bool CanExecute()
    {
        return Args.Count == 1 && _commandStack.Any();
    }

    public override StringBuilder ToBuffer()
    {
        return _topCommand.ToBuffer();
    }
}