using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;

namespace BashWrapperTests;

public static class FormatFixer
{
    public static void AreEqual(string expected, string actual)
    {
        AreEqual(SplitStringByNewLine(expected), SplitStringByNewLine(actual));
    }

    public static void AreEqual(IEnumerable<string> expected, string actual)
    {
        AreEqual(RemoveEmptyStrings(expected), SplitStringByNewLine(actual));
    }

    public static void AreEqual(string expected, IEnumerable<string> actual)
    {
        AreEqual(SplitStringByNewLine(expected), RemoveEmptyStrings(actual));
    }

    public static void AreEqual(IEnumerable<string> expected, IEnumerable<string> actual)
    {
        Assert.AreEqual(expected, actual);
    }

    private static IEnumerable<string> SplitStringByNewLine(string line)
    {
        var splitLine = line.Split(Environment.NewLine).ToList();
        return RemoveEmptyStrings(splitLine);
    }

    private static IEnumerable<string> RemoveEmptyStrings(IEnumerable<string> splitLine)
    {
        var lst = splitLine.ToList();
        lst.Remove("");
        return lst;
    }
}