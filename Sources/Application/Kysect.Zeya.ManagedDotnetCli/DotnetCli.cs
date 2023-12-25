using Kysect.CommonLib.BaseTypes.Extensions;
using System.Linq;
using Kysect.PowerShellRunner.Abstractions.Accessors;
using Kysect.PowerShellRunner.Abstractions.Objects;
using Kysect.PowerShellRunner.Abstractions.Queries;
using Kysect.PowerShellRunner.Executions;
using Kysect.Zeya.Abstractions;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;

namespace Kysect.Zeya.ManagedDotnetCli;

public class DotnetCli
{
    private readonly ILogger _logger;
    private readonly IPowerShellAccessor _powerShellAccessor;

    public DotnetCli(ILogger logger, IPowerShellAccessor powerShellAccessor)
    {
        _powerShellAccessor = powerShellAccessor.ThrowIfNull();
        _logger = logger;
    }

    public void Format(string pathToSolution, string pathToJson)
    {
        _logger.LogInformation("Generate warnings for {pathToSolution} and write result to {pathToJson}", pathToSolution, pathToJson);
        ExecuteNoResult($"dotnet format \"{pathToSolution}\" --verify-no-changes --report \"{pathToJson}\"");
    }

    public string GetProperty(string projectPath, string propertyName)
    {
        _logger.LogTrace("Execute dotnet build of {Project} for getting value of {Property}", projectPath, propertyName);
        return Execute($"dotnet build \"{projectPath}\" --getProperty:{propertyName}");
    }

    private void ExecuteNoResult(string command)
    {
        var powerShellQuery = new PowerShellQuery(command);
        _powerShellAccessor.ExecuteAndGet(powerShellQuery);
    }

    private string Execute(string command)
    {
        var powerShellQuery = new PowerShellQuery(command);
        IReadOnlyCollection<IPowerShellObject> result = _powerShellAccessor.ExecuteAndGet(powerShellQuery);

        if (result.Count != 1)
            throw new ZeyaException($"Unexpected response value element count: {result.Count}");

        return result.Single().ToString().ThrowIfNull();
    }
}