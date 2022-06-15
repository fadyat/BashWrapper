using BashWrapper.Outputs;

namespace BashWrapper;

public static class Program
{
    public static void Main()
    {
        var analyzer = new BashHandler(new ConsoleOutput());
        analyzer.AnalyzeRequests();
    }
}