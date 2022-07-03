using System.Collections.Immutable;

namespace BashWrapper.Outputs;

public class ListOutput : IOutput
{
    private readonly List<string> _outputBuffer;

    public ListOutput()
    {
        _outputBuffer = new List<string>();
    }

    public void Output(string value)
    {
        var splitValue = value.Split(Environment.NewLine);
        foreach (var v in splitValue)
        {
            if (v != string.Empty)
            {
                _outputBuffer.Add(v);
            }
        }
    }

    public ImmutableList<string> OutputBuffer => _outputBuffer.ToImmutableList();
}