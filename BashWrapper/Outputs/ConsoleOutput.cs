namespace BashWrapper.Outputs;

public class ConsoleOutput : IOutput
{
    public void Output(string value)
    {
        Console.WriteLine(value);
    }
}