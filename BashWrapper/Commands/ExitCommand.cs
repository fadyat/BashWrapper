using System.Collections.Immutable;
using System.Text;

namespace BashWrapper.Commands;

public class ExitCommand : AbstractCommand
{
    private readonly Dictionary<string, string> _assignedVariables;
    private string _result;

    public ExitCommand(ImmutableList<string> args, StringBuilder buffer, Dictionary<string, string> assignedVariables) : base(args, buffer)
    {
        _assignedVariables = assignedVariables;
        _result = string.Empty;
    }


    public override object Execute()
    {
        if (!CanExecute())
        {
            if (Args.Count != 1) throw new ArgumentException($"Wrong args number {Args.Count}, expected 1!");
            throw new ArgumentException("Exit code must be integer!");
        }

        _result = Args.Any() ? Args[0] : "0";
        _assignedVariables["$?"] = (_result == "0" ? ExitStatus.True : ExitStatus.False).ToString();
        return _result;
    }

    protected override bool CanExecute()
    {
        return Args.Count == 1 && int.TryParse(Args[0], out _) || !Args.Any();
    }

    public override StringBuilder ToBuffer()
    {
        Buffer.AppendLine(_result);
        return Buffer;
    }
}