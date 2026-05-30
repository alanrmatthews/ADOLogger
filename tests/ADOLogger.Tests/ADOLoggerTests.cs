using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace ADOLogger.Tests;

public class ADOLoggerTests
{
    private readonly ADOLogger logger;

    public ADOLoggerTests()
    {
        logger = new ADOLogger();
    }

    [Fact]
    public void CanInstantiateLogger()
    {
        Assert.NotNull(logger);
    }

    [Fact]
    public void CanLogWarningMessage()
    {
        var result = LogMessage(LogLevel.Warning, "Warning message");
        Assert.Contains("##[warning]Warning message", result);
    }

    [Fact]
    public void CanLogErrorMessage()
    {
        var result = LogMessage(LogLevel.Error, "Error message");
        Assert.Contains("##vso[task.logissue type=error]Error message", result);
    }

    [Fact]
    public void CanLogInfoMessage()
    {
        var result = LogMessage(LogLevel.Information, "Info message");
        Assert.Equal("Info message" + Environment.NewLine, result);
    }

    [Fact]
    public void LogIssue_WithWarning_WritesWarningFormat()
    {
        var output = new StringWriter();
        Console.SetOut(output);

        logger.LogIssue("Test warning", ADOIssueType.Warning);

        Assert.Contains("##vso[task.logissue type=warning]Test warning", output.ToString());
    }

    [Fact]
    public void LogIssue_WithError_WritesErrorFormat()
    {
        var output = new StringWriter();
        Console.SetOut(output);

        logger.LogIssue("Test error", ADOIssueType.Error);

        Assert.Contains("##vso[task.logissue type=error]Test error", output.ToString());
    }

    [Fact]
    public void LogWarning_WritesWarningFormat()
    {
        var output = new StringWriter();
        Console.SetOut(output);

        logger.LogWarning("Warning message");

        Assert.Contains("##vso[task.logissue type=warning]Warning message", output.ToString());
    }

    [Fact]
    public void LogError_WritesErrorFormat()
    {
        var output = new StringWriter();
        Console.SetOut(output);

        logger.LogError("Error message");

        Assert.Contains("##vso[task.logissue type=error]Error message", output.ToString());
    }

    // Helper method to reduce duplication
    private string LogMessage(LogLevel logLevel, string message)
    {
        var output = new StringWriter();
        var logEntry = new LogEntry<string>(
            logLevel,
            "TestCategory",
            new EventId(0),
            message,
            null,
            (state, exception) => state);

        logger.Write(logEntry, null, output);
        return output.ToString();
    }
}