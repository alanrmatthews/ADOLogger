using System.IO;

namespace ADOLogger.Tests;

public class ADOLoggerTests
{
    [Fact]
    public void CanInstantiateLogger()
    {
        var logger = new ADOLogger();

        Assert.NotNull(logger);
    }

    [Fact]
    public void Warning_WritesFormattingCommand()
    {
        var writer = new StringWriter();
        var logger = new ADOLogger(writer);

        var command = logger.Warning("Check this warning");

        Assert.Equal("##[warning]Check this warning", command);
        Assert.Equal(command + Environment.NewLine, writer.ToString());
    }

    [Fact]
    public void EndGroup_WritesEndGroupMarker()
    {
        var writer = new StringWriter();
        var logger = new ADOLogger(writer);

        var command = logger.EndGroup();

        Assert.Equal("##[endgroup]", command);
        Assert.Equal(command + Environment.NewLine, writer.ToString());
    }

    [Fact]
    public void SetVariable_EscapesReservedCharacters()
    {
        var writer = new StringWriter();
        var logger = new ADOLogger(writer);

        var command = logger.SetVariable("buildStatus", "50%; ready]\nnext", isOutput: true);

        Assert.Equal("##vso[task.setvariable variable=buildStatus;isOutput=true;]50%AZP25; ready]%0Anext", command);
    }

    [Fact]
    public void LogIssue_IncludesOptionalMetadata()
    {
        var writer = new StringWriter();
        var logger = new ADOLogger(writer);

        var command = logger.LogIssue("warning", "Potential issue", sourcePath: "src/file.cs", lineNumber: 4, columnNumber: 2, code: "CS1001");

        Assert.Equal("##vso[task.logissue type=warning;sourcepath=src/file.cs;linenumber=4;columnnumber=2;code=CS1001;]Potential issue", command);
    }

    [Fact]
    public void UploadArtifact_UsesContainerFolderWhenProvided()
    {
        var writer = new StringWriter();
        var logger = new ADOLogger(writer);

        var command = logger.UploadArtifact("drop", "c:/artifacts/result.trx", containerFolder: "testresult");

        Assert.Equal("##vso[artifact.upload containerfolder=testresult;artifactname=drop;]c:/artifacts/result.trx", command);
    }

    [Fact]
    public void SetEndpoint_RequiresKeyForNonUrlFields()
    {
        var logger = new ADOLogger(new StringWriter());

        var exception = Assert.Throws<ArgumentException>(() => logger.SetEndpoint("endpoint-id", "authParameter", "token"));

        Assert.Equal("key", exception.ParamName);
    }

    [Fact]
    public void AddBuildTag_RejectsColon()
    {
        var logger = new ADOLogger(new StringWriter());

        var exception = Assert.Throws<ArgumentException>(() => logger.AddBuildTag("release:1"));

        Assert.Equal("buildTag", exception.ParamName);
    }
}