using System.Collections.Immutable;
using System.Text;

namespace BashWrapper.Commands;

public class PwdCommand : AbstractCommand
{
    private string _result;

    public PwdCommand(ImmutableList<string> args, StringBuilder buffer) : base(args, buffer)
    {
        _result = string.Empty;
    }

    public override object Execute()
    {
        if (!CanExecute())
            throw new ArgumentException($"Too much args {Args.Count} expected 0!");

        _result = Directory.GetCurrentDirectory();
        return _result;
    }

    protected override bool CanExecute()
    {
        return !Args.Any();
    }

    public override StringBuilder ToBuffer()
    {
        Buffer.AppendLine(_result);
        return Buffer;
    }
}