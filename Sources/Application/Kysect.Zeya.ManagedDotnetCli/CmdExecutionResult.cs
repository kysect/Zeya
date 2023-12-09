using System;
using System.Collections.Generic;
using System.Linq;

namespace Kysect.Zeya.ManagedDotnetCli;

public class CmdExecutionResult
{
    public string StandardOutput { get; init; }
    public int ExitCode { get; init; }
    public IReadOnlyCollection<string> Errors { get; init; }

    public CmdExecutionResult(string standardOutput, int exitCode, IReadOnlyCollection<string> errors)
    {
        StandardOutput = standardOutput;
        ExitCode = exitCode;
        Errors = errors;
    }

    public void ThrowIfAnyError()
    {
        if (Errors.Count == 1)
            throw new DotnetCliException(Errors.Single());

        if (Errors.Count > 0)
        {
            var exceptions = Errors
                .Select(m => new DotnetCliException(m))
                .ToList();
            throw new AggregateException(exceptions);
        }

        if (ExitCode != 0)
            throw new DotnetCliException($"Return {ExitCode} exit code.");
    }

    public bool IsAnyError()
    {
        return Errors.Any() || ExitCode != 0;
    }
}