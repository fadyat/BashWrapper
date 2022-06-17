using BashWrapper.Outputs;

namespace BashWrapper.Handlers;

public class BashTestHandler : BashHandler
{
    public BashTestHandler(IOutput outputMethod) : base(outputMethod)
    {
    }

    public override void AnalyzeRequests()
    {
        var isFirstCommand = true;
        while (Running && isFirstCommand)
        {
            try
            {
                var inputCommand = Console.ReadLine();
                if (string.IsNullOrEmpty(inputCommand)) continue;
                var parsedCommand = CommandParser.Parse(inputCommand);
                var groupedCommands = CommandParser.SplitArgsByConnectorsAndRedirectors(parsedCommand);
                RunCommands(groupedCommands);
                OutputMethod.Output(Buffer.ToString());
                Buffer.Clear();
                isFirstCommand = false;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }
    }
}