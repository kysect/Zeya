using Microsoft.Extensions.Logging;

namespace Kysect.Zeya.ManagedDotnetCli;

public class DotnetCli
{
    private readonly ILogger _logger;
    private readonly CmdProcess _cmdProcess;

    public DotnetCli(ILogger logger)
    {
        _cmdProcess = new CmdProcess(logger);
        _logger = logger;
    }

    public void Format(string pathToSolution, string pathToJson)
    {
        _logger.LogInformation("Generate warnings for {pathToSolution} and write result to {pathToJson}", pathToSolution, pathToJson);
        // TODO: handle exceptions in some way?
        _cmdProcess.ExecuteCommand($"dotnet format \"{pathToSolution}\" --verify-no-changes --report \"{pathToJson}\"");
    }

    public string GetProperty(string projectPath, string propertyName)
    {
        _logger.LogTrace("Execute dotnet build of {Project} for getting value of {Property}", projectPath, propertyName);
        var result = _cmdProcess.ExecuteCommand($"dotnet build \'{projectPath}\' --getProperty:{propertyName}");
        return result.StandardOutput.Trim();
    }
}