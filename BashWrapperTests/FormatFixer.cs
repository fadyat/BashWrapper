using System.Collections.Generic;
using System.Text;

namespace BashWrapperTests;

public static class FormatFixer
{
    public static string FixExpectedStringFormat(string line)
    {
        return FixExpectedStringFormat(new[] {line});
    }

    public static string FixExpectedStringFormat(IEnumerable<string> lines)
    {
        var builder = new StringBuilder();
        foreach (var line in lines) builder.AppendLine(line);
        return builder.ToString();
    }
}