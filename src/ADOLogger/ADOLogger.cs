using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Logging.Console;

namespace ADOLogger;

/// <summary>
/// Azure DevOps Pipeline log issue types.
/// </summary>
public enum ADOIssueType
{
    /// <summary>Warning level issue.</summary>
    Warning,
    /// <summary>Error level issue.</summary>
    Error
}

/// <summary>
/// Console formatter for Azure DevOps Pipeline logging commands.
/// </summary>
public sealed class ADOLogger : ConsoleFormatter
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ADOLogger"/> class.
    /// </summary>
    public ADOLogger() : base("ado")
    {
    }

    public override void Write<TState>(in LogEntry<TState> logEntry, IExternalScopeProvider? scopeProvider, TextWriter writer)
    {
        var message = logEntry.Formatter(logEntry.State, logEntry.Exception);

        if (string.IsNullOrEmpty(message))
            return;

        switch (logEntry.LogLevel)
        {
            case LogLevel.Error:
            case LogLevel.Critical:
                writer.WriteLine($"##vso[task.logissue type=error]{message}");
                break;

            case LogLevel.Warning:
                writer.WriteLine($"##[warning]{message}");
                break;

            default:
                writer.WriteLine(message);
                break;
        }
    }

    /// <summary>
    /// Logs an issue to Azure DevOps Pipeline.
    /// </summary>
    /// <param name="message">The issue message.</param>
    /// <param name="type">The issue type (Warning or Error).</param>
    public void LogIssue(string message, ADOIssueType type)
    {
        var typeString = type switch
        {
            ADOIssueType.Warning => "warning",
            ADOIssueType.Error => "error",
            _ => throw new ArgumentOutOfRangeException(nameof(type), type, "Invalid issue type")
        };

        Console.WriteLine($"##vso[task.logissue type={typeString}]{message}");
    }

    /// <summary>
    /// Logs a warning message to Azure DevOps Pipeline.
    /// </summary>
    /// <param name="message">The warning message.</param>
    public void LogWarning(string message)
    {
        LogIssue(message, ADOIssueType.Warning);
    }

    /// <summary>
    /// Logs an error message to Azure DevOps Pipeline.
    /// </summary>
    /// <param name="message">The error message.</param>
    public void LogError(string message)
    {
        LogIssue(message, ADOIssueType.Error);
    }

    ////////////////////////////////////////////////////////////////////////////////
    /// Build Commands                                                            //
    ////////////////////////////////////////////////////////////////////////////////
    // AddBuildTag
    // UpdateBuildNumber
    // UploadLog

    ////////////////////////////////////////////////////////////////////////////////
    /// Release Commands                                                          //
    ////////////////////////////////////////////////////////////////////////////////
    /// <summary>
    /// Updates the release name in Azure DevOps Pipeline.
    /// </summary>
    /// <param name="releaseName">The new release name.</param>
    public void UpdateReleaseName(string releaseName)
    {
        Console.WriteLine($"##vso[release.updateReleaseName]{releaseName}");
    }
}