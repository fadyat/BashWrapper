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

    [SetUp]
    public void Setup()
    {
        _currentDirectory = Directory.GetCurrentDirectory();
        _fileName = "tmp.txt";
        _filePath = Path.Combine(_currentDirectory, _fileName);
        using (File.Create(Path.Combine(_currentDirectory, _fileName))) { }
    }
    
    [Test]
    public void MultipleLsTest()
    {
        File.WriteAllLines(_filePath, new [] {"ls", "ls .."});
        var listOutput = new ListOutput();
        var analyzer = new BashFileHandler(listOutput, _filePath);
        analyzer.AnalyzeRequests();
        var currentEntries = Directory.GetFileSystemEntries(_currentDirectory);
        var parentEntries = Directory.GetFileSystemEntries(Path.GetFullPath(Path.Combine(_currentDirectory, "..")));
        var expected = currentEntries.Concat(parentEntries).ToList();
        var actual = listOutput.OutputBuffer;
        FormatFixer.AreEqual(expected, actual);
    }
}