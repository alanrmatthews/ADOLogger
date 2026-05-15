using System.Globalization;
using System.IO;
using System.Text;

namespace ADOLogger;

public class ADOLogger
{
    private static readonly HashSet<string> ValidIssueTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        "error",
        "warning"
    };

    private static readonly HashSet<string> ValidTaskResults = new(StringComparer.OrdinalIgnoreCase)
    {
        "Succeeded",
        "SucceededWithIssues",
        "Failed"
    };

    private static readonly HashSet<string> ValidTimelineStates = new(StringComparer.OrdinalIgnoreCase)
    {
        "Unknown",
        "Initialized",
        "InProgress",
        "Completed"
    };

    private static readonly HashSet<string> ValidTimelineResults = new(StringComparer.OrdinalIgnoreCase)
    {
        "Succeeded",
        "SucceededWithIssues",
        "Failed"
    };

    private static readonly HashSet<string> ValidEndpointFields = new(StringComparer.OrdinalIgnoreCase)
    {
        "authParameter",
        "dataParameter",
        "url"
    };

    private static readonly HashSet<string> ValidArtifactTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        "container",
        "filepath",
        "versioncontrol",
        "gitref",
        "tfvclabel"
    };

    private readonly TextWriter output;

    public ADOLogger(TextWriter? output = null)
    {
        this.output = output ?? Console.Out;
    }

    public string Group(string message) => WriteFormattingCommand("group", message);

    public string Warning(string message) => WriteFormattingCommand("warning", message);

    public string Error(string message) => WriteFormattingCommand("error", message);

    public string Section(string message) => WriteFormattingCommand("section", message);

    public string Debug(string message) => WriteFormattingCommand("debug", message);

    public string Command(string message) => WriteFormattingCommand("command", message);

    public string EndGroup()
    {
        const string command = "##[endgroup]";
        output.WriteLine(command);
        return command;
    }

    public string LogIssue(
        string type,
        string message,
        string? sourcePath = null,
        int? lineNumber = null,
        int? columnNumber = null,
        string? code = null)
    {
        EnsureAllowedValue(type, ValidIssueTypes, nameof(type));

        return WriteServiceCommand(
            "task",
            "logissue",
            message,
            ("type", type),
            ("sourcepath", sourcePath),
            ("linenumber", lineNumber?.ToString(CultureInfo.InvariantCulture)),
            ("columnnumber", columnNumber?.ToString(CultureInfo.InvariantCulture)),
            ("code", code));
    }

    public string SetProgress(int value, string currentOperation)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(value, 0);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(value, 100);

        return WriteServiceCommand(
            "task",
            "setprogress",
            currentOperation,
            ("value", value.ToString(CultureInfo.InvariantCulture)));
    }

    public string Complete(string? currentOperation = null, string result = "Succeeded")
    {
        EnsureAllowedValue(result, ValidTaskResults, nameof(result));

        return WriteServiceCommand(
            "task",
            "complete",
            currentOperation ?? string.Empty,
            ("result", result));
    }

    public string LogDetail(
        Guid id,
        string? message = null,
        Guid? parentId = null,
        string? type = null,
        string? name = null,
        int? order = null,
        DateTimeOffset? startTime = null,
        DateTimeOffset? finishTime = null,
        int? progress = null,
        string? state = null,
        string? result = null)
    {
        if (progress is < 0 or > 100)
        {
            throw new ArgumentOutOfRangeException(nameof(progress), "Progress must be between 0 and 100.");
        }

        if (state is not null)
        {
            EnsureAllowedValue(state, ValidTimelineStates, nameof(state));
        }

        if (result is not null)
        {
            EnsureAllowedValue(result, ValidTimelineResults, nameof(result));
        }

        return WriteServiceCommand(
            "task",
            "logdetail",
            message ?? string.Empty,
            ("id", id.ToString()),
            ("parentid", parentId?.ToString()),
            ("type", type),
            ("name", name),
            ("order", order?.ToString(CultureInfo.InvariantCulture)),
            ("starttime", startTime?.ToString("O", CultureInfo.InvariantCulture)),
            ("finishtime", finishTime?.ToString("O", CultureInfo.InvariantCulture)),
            ("progress", progress?.ToString(CultureInfo.InvariantCulture)),
            ("state", state),
            ("result", result));
    }

    public string SetVariable(string variable, string value, bool isSecret = false, bool isOutput = false, bool isReadOnly = false)
    {
        EnsureRequired(variable, nameof(variable));

        return WriteServiceCommand(
            "task",
            "setvariable",
            value,
            ("variable", variable),
            ("isSecret", isSecret ? "true" : null),
            ("isOutput", isOutput ? "true" : null),
            ("isReadOnly", isReadOnly ? "true" : null));
    }

    public string SetSecret(string value) => WriteServiceCommand("task", "setsecret", value);

    public string SetEndpoint(string id, string field, string value, string? key = null)
    {
        EnsureRequired(id, nameof(id));
        EnsureAllowedValue(field, ValidEndpointFields, nameof(field));

        if (!field.Equals("url", StringComparison.OrdinalIgnoreCase))
        {
            EnsureRequired(key, nameof(key));
        }

        return WriteServiceCommand(
            "task",
            "setendpoint",
            value,
            ("id", id),
            ("field", field),
            ("key", key));
    }

    public string AddAttachment(string type, string name, string filePath)
    {
        EnsureRequired(type, nameof(type));
        EnsureRequired(name, nameof(name));

        return WriteServiceCommand(
            "task",
            "addattachment",
            filePath,
            ("type", type),
            ("name", name));
    }

    public string UploadSummary(string filePath) => WriteServiceCommand("task", "uploadsummary", filePath);

    public string UploadFile(string filePath) => WriteServiceCommand("task", "uploadfile", filePath);

    public string PrependPath(string path) => WriteServiceCommand("task", "prependpath", path);

    public string AssociateArtifact(string artifactName, string artifactLocation, string type)
    {
        EnsureRequired(artifactName, nameof(artifactName));
        EnsureAllowedValue(type, ValidArtifactTypes, nameof(type));

        return WriteServiceCommand(
            "artifact",
            "associate",
            artifactLocation,
            ("artifactname", artifactName),
            ("type", type));
    }

    public string AssociateCustomArtifact(string artifactName, string artifactLocation, string artifactType)
    {
        EnsureRequired(artifactName, nameof(artifactName));
        EnsureRequired(artifactType, nameof(artifactType));

        return WriteServiceCommand(
            "artifact",
            "associate",
            artifactLocation,
            ("artifactname", artifactName),
            ("artifacttype", artifactType));
    }

    public string UploadArtifact(string artifactName, string filePath, string? containerFolder = null)
    {
        EnsureRequired(artifactName, nameof(artifactName));

        return WriteServiceCommand(
            "artifact",
            "upload",
            filePath,
            ("containerfolder", containerFolder),
            ("artifactname", artifactName));
    }

    public string UploadLog(string filePath) => WriteServiceCommand("build", "uploadlog", filePath);

    public string UpdateBuildNumber(string buildNumber) => WriteServiceCommand("build", "updatebuildnumber", buildNumber);

    public string AddBuildTag(string buildTag)
    {
        EnsureRequired(buildTag, nameof(buildTag));

        if (buildTag.Contains(':', StringComparison.Ordinal))
        {
            throw new ArgumentException("Build tags cannot contain ':'.", nameof(buildTag));
        }

        return WriteServiceCommand("build", "addbuildtag", buildTag);
    }

    public string UpdateReleaseName(string releaseName) => WriteServiceCommand("release", "updatereleasename", releaseName);

    private string WriteFormattingCommand(string command, string message)
    {
        EnsureRequired(command, nameof(command));
        var formattedCommand = $"##[{command}]{EscapeMessage(message)}";
        output.WriteLine(formattedCommand);
        return formattedCommand;
    }

    private string WriteServiceCommand(string area, string action, string message, params (string Name, string? Value)[] properties)
    {
        EnsureRequired(area, nameof(area));
        EnsureRequired(action, nameof(action));

        var builder = new StringBuilder();
        builder.Append("##vso[");
        builder.Append(area);
        builder.Append('.');
        builder.Append(action);

        if (properties.Length > 0)
        {
            builder.Append(' ');

            foreach (var (name, value) in properties)
            {
                if (value is null)
                {
                    continue;
                }

                builder.Append(name);
                builder.Append('=');
                builder.Append(EscapePropertyValue(value));
                builder.Append(';');
            }
        }

        builder.Append(']');
        builder.Append(EscapeMessage(message));

        var command = builder.ToString();
        output.WriteLine(command);
        return command;
    }

    private static string EscapePropertyValue(string value)
    {
        return EscapePercent(value)
            .Replace(";", "%3B", StringComparison.Ordinal)
            .Replace("\r", "%0D", StringComparison.Ordinal)
            .Replace("\n", "%0A", StringComparison.Ordinal)
            .Replace("]", "%5D", StringComparison.Ordinal);
    }

    private static string EscapeMessage(string value)
    {
        return EscapePercent(value)
            .Replace("\r", "%0D", StringComparison.Ordinal)
            .Replace("\n", "%0A", StringComparison.Ordinal);
    }

    private static string EscapePercent(string value) => value.Replace("%", "%AZP25", StringComparison.Ordinal);

    private static void EnsureAllowedValue(string value, IReadOnlySet<string> validValues, string paramName)
    {
        EnsureRequired(value, paramName);

        if (!validValues.Contains(value))
        {
            throw new ArgumentException($"Unexpected value '{value}'.", paramName);
        }
    }

    private static void EnsureRequired(string? value, string paramName)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException("A value is required.", paramName);
        }
    }
}