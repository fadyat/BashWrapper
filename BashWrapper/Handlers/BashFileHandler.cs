using System.Collections.Immutable;
using BashWrapper.Outputs;

namespace BashWrapper.Handlers;

public class BashFileHandler : BashHandler
{
    private readonly ImmutableList<string> _lines;
    
    public BashFileHandler(IOutput outputMethod, string filePath) : base(outputMethod)
    {
        if (!File.Exists(filePath)) throw new FileNotFoundException($"{filePath} doesn't exists!");
        _lines = File.ReadAllLines(filePath).ToImmutableList();
    }

    public override void AnalyzeRequests()
    {
        foreach (var inputCommand in _lines)
        {
            try
            {
                if (string.IsNullOrEmpty(inputCommand)) continue;
                var parsedCommand = CommandParser.Parse(inputCommand);
                var groupedCommands = CommandParser.SplitArgsByConnectorsAndRedirectors(parsedCommand);
                RunCommands(groupedCommands);
                OutputMethod.Output(Buffer.ToString());
                Buffer.Clear();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }
    }
}