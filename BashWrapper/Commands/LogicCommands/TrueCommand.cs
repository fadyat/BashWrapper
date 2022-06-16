using System.Collections.Immutable;
using System.Text;

namespace BashWrapper.Commands.LogicCommands;

public class TrueCommand : AbstractCommand
{
    private readonly Dictionary<string, string> _assignedVariables;

    public TrueCommand(ImmutableList<string> args, StringBuilder buffer, Dictionary<string, string> assignedVariables)
        : base(args, buffer)
    {
        _assignedVariables = assignedVariables;
    }

    public override object Execute()
    {
        _assignedVariables["$?"] = ExitStatus.True.ToString();
        return _assignedVariables;
    }

    protected override bool CanExecute()
    {
        return true;
    }

    public override StringBuilder ToBuffer()
    {
        return new StringBuilder();
    }
}