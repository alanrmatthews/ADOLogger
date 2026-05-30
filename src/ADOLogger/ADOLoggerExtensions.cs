using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Console;

namespace ADOLogger;

/// <summary>
/// Extension methods for configuring ADO (Azure DevOps) logging.
/// </summary>
public static class ADOLoggerExtensions
{
    /// <summary>
    /// Adds an ADO (Azure DevOps) console formatter to the logging builder.
    /// Formats log messages using Azure DevOps Pipeline logging commands.
    /// </summary>
    /// <param name="builder">The <see cref="ILoggingBuilder"/> to configure.</param>
    /// <returns>The <see cref="ILoggingBuilder"/> for method chaining.</returns>
    public static ILoggingBuilder AddADOLogger(this ILoggingBuilder builder)
    {
        return builder.AddConsole(options => options.FormatterName = "ado")
                      .AddConsoleFormatter<ADOLogger, ConsoleFormatterOptions>();
    }

    /// <summary>
    /// Adds an ADO (Azure DevOps) console formatter with custom options.
    /// </summary>
    /// <param name="builder">The <see cref="ILoggingBuilder"/> to configure.</param>
    /// <param name="configure">A delegate to configure the <see cref="ConsoleFormatterOptions"/>.</param>
    /// <returns>The <see cref="ILoggingBuilder"/> for method chaining.</returns>
    public static ILoggingBuilder AddADOLogger(
        this ILoggingBuilder builder, 
        Action<ConsoleFormatterOptions> configure)
    {
        return builder.AddConsole(options => options.FormatterName = "ado")
                      .AddConsoleFormatter<ADOLogger, ConsoleFormatterOptions>(configure);
    }
}