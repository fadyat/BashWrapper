using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using BashWrapper;
using BashWrapper.Outputs;
using NUnit.Framework;

namespace BashWrapperTests;

public class Tests
{
    private BashHandler _analyzer;
    private string _currentDirectory;
    private string _filePath;
    private string _fileName;
    private StringWriter _stringWriter;

    [SetUp]
    public void Setup()
    {
        _currentDirectory = Directory.GetCurrentDirectory();
        _analyzer = new BashHandler(new ConsoleOutput());
        _fileName = "tmp.txt";
        _filePath = Path.Combine(_currentDirectory, _fileName);
        using (File.Create(Path.Combine(_currentDirectory, _fileName)))
        {
        }

        File.WriteAllLines(_filePath, new[] {"first line", "second line", "aboba"});
        _stringWriter = new StringWriter();
        Console.SetOut(_stringWriter);
    }

    [Test]
    public void ExitTest()
    {
        var stringReader = new StringReader("exit");
        Console.SetIn(stringReader);

        _analyzer.AnalyzeRequests(true);

        const string expected = "0";
        var actual = _stringWriter.ToString()[..expected.Length];
        Assert.AreEqual(expected, actual);
    }

    [Test]
    public void PwdTest()
    {
        var stringReader = new StringReader("pwd");
        Console.SetIn(stringReader);

        _analyzer.AnalyzeRequests(true);

        var expected = Directory.GetCurrentDirectory();
        var actual = _stringWriter.ToString()[..expected.Length];
        Assert.AreEqual(expected, actual);
    }

    [Test]
    public void LsTest()
    {
        var stringReader = new StringReader("ls");
        Console.SetIn(stringReader);

        _analyzer.AnalyzeRequests(true);

        var expected = string.Join("\n", Directory.GetFileSystemEntries(_currentDirectory));
        var actual = _stringWriter.ToString()[..expected.Length];
        Assert.AreEqual(expected, actual);
    }

    // ls test multiple can be here :)

    [Test]
    public void CatTest()
    {
        var stringReader = new StringReader($"cat {_fileName}");
        Console.SetIn(stringReader);
        _analyzer.AnalyzeRequests(true);
        var expected = string.Join("\n", File.ReadAllLines(_filePath));
        var actual = _stringWriter.ToString()[..expected.Length];
        Assert.AreEqual(expected, actual);
    }

    [Test]
    public void CatTestMultiple()
    {
        var stringReaderMultiple = new StringReader($"cat {_fileName} {_fileName} {_fileName}");
        Console.SetIn(stringReaderMultiple);
        _analyzer.AnalyzeRequests(true);
        var contentMultiple = new List<string>();
        foreach (var content in new List<string> {_fileName, _fileName, _fileName}.Select(File.ReadAllLines))
            contentMultiple.AddRange(content);
        var expectedMultiple = string.Join("\n", contentMultiple);
        var actualMultiple = _stringWriter.ToString()[..expectedMultiple.Length];
        Assert.AreEqual(expectedMultiple, actualMultiple);
    }

    // Further fileName will be used as a string
    [Test]
    public void EchoTest()
    {
        var stringReader = new StringReader($"echo {_fileName}");
        Console.SetIn(stringReader);
        _analyzer.AnalyzeRequests(true);
        var expected = _fileName;
        var actual = _stringWriter.ToString()[..expected.Length];
        Assert.AreEqual(expected, actual);
    }

    [Test]
    public void EchoTestMultiple()
    {
        var stringReader = new StringReader($"echo {_fileName} {_fileName} {_fileName}");
        Console.SetIn(stringReader);
        _analyzer.AnalyzeRequests(true);
        var expected = string.Join(" ", new List<string> {_fileName, _fileName, _fileName});
        var actual = _stringWriter.ToString()[..expected.Length];
        Assert.AreEqual(expected, actual);
    }

    [Test]
    public void TestExitStatusWithTrueCommand()
    {
        var stringReader = new StringReader("true ; echo $?");
        Console.SetIn(stringReader);
        _analyzer.AnalyzeRequests(true);
        const string expected = "0";
        var actual = _stringWriter.ToString()[..expected.Length];
        Assert.AreEqual(expected, actual);
    }

    [Test]
    public void TestExitStatusWithFalseCommand()
    {
        var stringReader = new StringReader("false ; echo $?");
        Console.SetIn(stringReader);
        _analyzer.AnalyzeRequests(true);
        const string expected = "0";
        var actual = _stringWriter.ToString()[..expected.Length];
        Assert.AreEqual(expected, actual);
    }

    [Test]
    public void CheckLocalVariablesAssignment()
    {
        const string expected = "aboba";
        var stringReader = new StringReader($"$tmp={expected} ; echo $tmp");
        Console.SetIn(stringReader);
        _analyzer.AnalyzeRequests(true);
        var actual = _stringWriter.ToString()[..expected.Length];
        Assert.AreEqual(expected, actual);
    }

    [Test]
    public void WcTest()
    {
        // ...
    }

    [Test]
    public void AndConnectorTestTrue()
    {
        const string expected = "aboba";
        var stringReader = new StringReader($"true && echo {expected}");
        Console.SetIn(stringReader);
        _analyzer.AnalyzeRequests(true);
        var actual = _stringWriter.ToString()[..expected.Length];
        Assert.AreEqual(expected, actual);
    }

    [Test]
    public void AndConnectorTestFalse()
    {
        const string expected = "aboba";
        var stringReader = new StringReader($"false && echo {expected}");
        Console.SetIn(stringReader);
        _analyzer.AnalyzeRequests(true);
        var actual = _stringWriter.ToString();
        actual = actual.Remove(actual.Length - 1, 1);
        Assert.AreEqual(string.Empty, actual);
    }

    [Test]
    public void OrConnectorTestTrue()
    {
        const string expected = "aboba";
        var stringReader = new StringReader($"false || echo {expected}");
        Console.SetIn(stringReader);
        _analyzer.AnalyzeRequests(true);
        var actual = _stringWriter.ToString()[..expected.Length];
        Assert.AreEqual(expected, actual);
    }

    [Test]
    public void OrConnectorTestFalse()
    {
        const string expected = "aboba";
        var stringReader = new StringReader($"true || echo {expected}");
        Console.SetIn(stringReader);
        _analyzer.AnalyzeRequests(true);
        var actual = _stringWriter.ToString();
        actual = actual.Remove(actual.Length - 1, 1);
        Assert.AreEqual(string.Empty, actual);
    }

    [Test]
    public void OutputRedirection()
    {
        var stringReader = new StringReader($"echo aboba > {_fileName}");
        Console.SetIn(stringReader);
        _analyzer.AnalyzeRequests(true);
        var fileContent = string.Join("\n", File.ReadAllLines(_filePath));
        Assert.AreEqual("aboba", fileContent);
    }

    [Test]
    public void OutputRedirectionWithoutClean()
    {
        var stringReader = new StringReader($"echo aboba > {_fileName} && echo aboba >> {_fileName}");
        Console.SetIn(stringReader);
        _analyzer.AnalyzeRequests(true);
        var fileContent = string.Join("\n", File.ReadAllLines(_filePath));
        Assert.AreEqual("aboba\naboba", fileContent);
    }

    [Test]
    public void InputRedirection()
    {
        var stringReader = new StringReader($"cat < {_fileName}");
        Console.SetIn(stringReader);
        _analyzer.AnalyzeRequests(true);
        var expected = string.Join("\n", File.ReadAllLines(_filePath));
        var actual = _stringWriter.ToString()[..expected.Length];
        Assert.AreEqual(expected, actual);
    }

    [Test]
    public void InputAndOutputRedirection()
    {
        var stringReader = new StringReader($"cat < {_fileName} > tmp2.txt");
        Console.SetIn(stringReader);
        _analyzer.AnalyzeRequests(true);
        var expected = string.Join("\n", File.ReadAllLines(Path.Combine(_currentDirectory, "tmp2.txt")));
        var actual = string.Join("\n", File.ReadAllLines(_filePath));
        Assert.AreEqual(expected, actual);
    }
}