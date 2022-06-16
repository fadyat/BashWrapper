using System.Collections.Immutable;
using System.Text;

namespace BashWrapper.Commands.LogicCommands;

public class FalseCommand : AbstractCommand
{
    private readonly Dictionary<string, string> _assignedVariables;

    public FalseCommand(ImmutableList<string> args, StringBuilder buffer, Dictionary<string, string> assignedVariables)
        : base(args, buffer)
    {
        _assignedVariables = assignedVariables;
    }

    public override object Execute()
    {
        _assignedVariables["$?"] = ((int) ExitStatus.False).ToString();
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