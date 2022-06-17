using BashWrapper.Handlers;
using BashWrapper.Outputs;

namespace BashWrapper;

public static class Program
{
    public static void Main()
    {
        // var analyzer = new BashHandler(new ConsoleOutput());
        // analyzer.AnalyzeRequests();
        var analyzer = new BashFileHandler(new ConsoleOutput(), "/Users/artyomfadeyev/GitHub/BashWrapper/tmp.txt");
        analyzer.AnalyzeRequests();
    }
}