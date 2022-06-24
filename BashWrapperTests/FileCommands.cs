using System;
using System.IO;
using System.Linq;
using BashWrapper.Handlers;
using BashWrapper.Outputs;
using NUnit.Framework;

namespace BashWrapperTests;

public class FileCommands
{
    private string _currentDirectory = null!;
    private string _filePath = null!;
    private string _fileName = null!;
    private StringWriter _stringWriter = null!;

    [SetUp]
    public void Setup()
    {
        _currentDirectory = Directory.GetCurrentDirectory();
        _fileName = "tmp.txt";
        _filePath = Path.Combine(_currentDirectory, _fileName);
        using (File.Create(Path.Combine(_currentDirectory, _fileName))) { }
        _stringWriter = new StringWriter();
        Console.SetOut(_stringWriter);
    }
    
    [Test]
    public void MultipleLsTest()
    {
        File.WriteAllLines(_filePath, new [] {"ls", "ls .."});
        var analyzer = new BashFileHandler(new ConsoleOutput(), _filePath);
        analyzer.AnalyzeRequests();
        var currentEntries = Directory.GetFileSystemEntries(_currentDirectory);
        var parentEntries = Directory.GetFileSystemEntries(Path.GetFullPath(Path.Combine(_currentDirectory, "..")));
        var expected = currentEntries.Concat(parentEntries).ToList();
        var actual = _stringWriter.ToString();
        FormatFixer.AreEqual(expected, actual);
    }
}