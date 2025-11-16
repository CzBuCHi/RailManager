using Serilog;

namespace RailManager.Extensions;

public static class LoggerExtensions
{
    public static ILogger ForSourceContext(this ILogger? logger, string? scope = null) =>
        logger?.ForContext("SourceContext", scope ?? "Railroader.ModManager")!;
}
