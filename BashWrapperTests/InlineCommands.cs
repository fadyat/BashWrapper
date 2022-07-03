using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using BashWrapper.Handlers;
using BashWrapper.Outputs;
using NUnit.Framework;

namespace BashWrapperTests;

public class Tests
{
    private BashTestHandler _analyzer = null!;
    private string _currentDirectory = null!;
    private string _filePath = null!;
    private string _fileName = null!;
    private ListOutput _listOutput = null!;

    [SetUp]
    public void Setup()
    {
        _currentDirectory = Directory.GetCurrentDirectory();
        _listOutput = new ListOutput();
        _analyzer = new BashTestHandler(_listOutput);
        _fileName = "tmp.txt";
        _filePath = Path.Combine(_currentDirectory, _fileName);
        using (File.Create(Path.Combine(_currentDirectory, _fileName)))
        {
        }

        File.WriteAllLines(_filePath, new[] { "first line", "second line", "aboba" });
    }

    [Test]
    public void ExitTest()
    {
        var stringReader = new StringReader("exit");
        Console.SetIn(stringReader);
        _analyzer.AnalyzeRequests();
        const string expected = "0";
        var actual = _listOutput.OutputBuffer;
        FormatFixer.AreEqual(expected, actual);
    }

    [Test]
    public void PwdTest()
    {
        var stringReader = new StringReader("pwd");
        Console.SetIn(stringReader);
        _analyzer.AnalyzeRequests();
        var expected = Directory.GetCurrentDirectory();
        var actual = _listOutput.OutputBuffer;
        FormatFixer.AreEqual(expected, actual);
    }

    [Test]
    public void LsTest()
    {
        var stringReader = new StringReader("ls");
        Console.SetIn(stringReader);
        _analyzer.AnalyzeRequests();
        var expected = Directory.GetFileSystemEntries(_currentDirectory);
        var actual = _listOutput.OutputBuffer;
        FormatFixer.AreEqual(expected, actual);
    }

    // ls test multiple can be here :)

    [Test]
    public void CatTest()
    {
        var stringReader = new StringReader($"cat {_fileName}");
        Console.SetIn(stringReader);
        _analyzer.AnalyzeRequests();
        var expected = File.ReadAllLines(_fileName);
        var actual = _listOutput.OutputBuffer;
        FormatFixer.AreEqual(expected, actual);
    }

    [Test]
    public void CatTestMultiple()
    {
        var stringReaderMultiple = new StringReader($"cat {_fileName} {_fileName} {_fileName}");
        Console.SetIn(stringReaderMultiple);
        _analyzer.AnalyzeRequests();
        var expected = new List<string>();
        foreach (var content in new List<string> { _fileName, _fileName, _fileName }.Select(File.ReadAllLines))
            expected.AddRange(content);
        var actual = _listOutput.OutputBuffer;
        FormatFixer.AreEqual(expected, actual);
    }

    // Further fileName will be used as a string
    [Test]
    public void EchoTest()
    {
        var stringReader = new StringReader($"echo {_fileName}");
        Console.SetIn(stringReader);
        _analyzer.AnalyzeRequests();
        var expected = _fileName;
        var actual = _listOutput.OutputBuffer;
        FormatFixer.AreEqual(expected, actual);
    }

    [Test]
    public void EchoTestMultiple()
    {
        var stringReader = new StringReader($"echo {_fileName} {_fileName} {_fileName}");
        Console.SetIn(stringReader);
        _analyzer.AnalyzeRequests();
        var expected = string.Join(" ", new List<string> { _fileName, _fileName, _fileName });
        var actual = _listOutput.OutputBuffer;
        FormatFixer.AreEqual(expected, actual);
    }

    [Test]
    public void TestExitStatusWithTrueCommand()
    {
        var stringReader = new StringReader("true ; echo $?");
        Console.SetIn(stringReader);
        _analyzer.AnalyzeRequests();
        const string expected = "0";
        var actual = _listOutput.OutputBuffer;
        FormatFixer.AreEqual(expected, actual);
    }

    [Test]
    public void TestExitStatusWithFalseCommand()
    {
        var stringReader = new StringReader("false ; echo $?");
        Console.SetIn(stringReader);
        _analyzer.AnalyzeRequests();
        const string expected = "0";
        var actual = _listOutput.OutputBuffer;
        FormatFixer.AreEqual(expected, actual);
    }

    [Test]
    public void CheckLocalVariablesAssignment()
    {
        const string expected = "aboba";
        var stringReader = new StringReader($"$tmp={expected} ; echo $tmp");
        Console.SetIn(stringReader);
        _analyzer.AnalyzeRequests();
        var actual = _listOutput.OutputBuffer;
        FormatFixer.AreEqual(expected, actual);
    }

    [Test]
    public void AndConnectorTestTrue()
    {
        const string expected = "aboba";
        var stringReader = new StringReader($"true && echo {expected}");
        Console.SetIn(stringReader);
        _analyzer.AnalyzeRequests();
        var actual = _listOutput.OutputBuffer;
        FormatFixer.AreEqual(expected, actual);
    }

    [Test]
    public void AndConnectorTestFalse()
    {
        const string value = "aboba";
        var stringReader = new StringReader($"false && echo {value}");
        Console.SetIn(stringReader);
        _analyzer.AnalyzeRequests();
        var expected = string.Empty;
        var actual = _listOutput.OutputBuffer;
        FormatFixer.AreEqual(expected, actual);
    }

    [Test]
    public void OrConnectorTestTrue()
    {
        const string expected = "aboba";
        var stringReader = new StringReader($"false || echo {expected}");
        Console.SetIn(stringReader);
        _analyzer.AnalyzeRequests();
        var actual = _listOutput.OutputBuffer;
        FormatFixer.AreEqual(expected, actual);
    }

    [Test]
    public void OrConnectorTestFalse()
    {
        const string value = "aboba";
        var stringReader = new StringReader($"true || echo {value}");
        Console.SetIn(stringReader);
        _analyzer.AnalyzeRequests();
        var expected = string.Empty;
        var actual = _listOutput.OutputBuffer;
        FormatFixer.AreEqual(expected, actual);
    }

    [Test]
    public void OutputRedirection()
    {
        const string expected = "aboba";
        var stringReader = new StringReader($"echo {expected} > {_fileName}");
        Console.SetIn(stringReader);
        _analyzer.AnalyzeRequests();
        var actual = File.ReadAllLines(_filePath);
        FormatFixer.AreEqual(expected, actual);
    }

    [Test]
    public void OutputRedirectionWithoutClean()
    {
        var stringReader = new StringReader($"echo aboba > {_fileName} && echo aboba >> {_fileName}");
        Console.SetIn(stringReader);
        _analyzer.AnalyzeRequests();
        var expected = new[] { "aboba", "aboba" };
        var actual = File.ReadAllLines(_filePath);
        FormatFixer.AreEqual(expected, actual);
    }

    [Test]
    public void InputRedirection()
    {
        var stringReader = new StringReader($"cat < {_fileName}");
        Console.SetIn(stringReader);
        _analyzer.AnalyzeRequests();
        var expected = File.ReadAllLines(_filePath);
        var actual = _listOutput.OutputBuffer;
        FormatFixer.AreEqual(expected, actual);
    }

    [Test]
    public void InputAndOutputRedirection()
    {
        var stringReader = new StringReader($"cat < {_fileName} > tmp2.txt");
        Console.SetIn(stringReader);
        _analyzer.AnalyzeRequests();
        var expected = File.ReadAllLines(Path.Combine(_currentDirectory, "tmp2.txt"));
        var actual = File.ReadAllLines(_filePath);
        FormatFixer.AreEqual(expected, actual);
    }
}