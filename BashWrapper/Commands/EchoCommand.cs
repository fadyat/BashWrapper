using System.Collections.Immutable;
using System.Text;

namespace BashWrapper.Commands;

// works like simple system.output; don't work with regex
public class EchoCommand : AbstractCommand
{
    private string _result;
    
    public EchoCommand(ImmutableList<string> args, StringBuilder buffer) : base(args, buffer)
    {
        _result = string.Empty;
    }

    public override object Execute()
    {
        if (!CanExecute()) return 0; // future checks may written here
        _result = string.Join(" ", Args);
        return _result;
    }

    protected override bool CanExecute()
    {
        return true;
    }

    public override StringBuilder ToBuffer()
    {
        Buffer.AppendLine(_result);
        return Buffer;
    }
}