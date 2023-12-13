using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using Microsoft.Extensions.Logging;

namespace Kysect.Zeya.ManagedDotnetCli;

public class CmdProcess
{
    private static TimeSpan _timeout = TimeSpan.FromSeconds(20);
    private readonly ILogger _logger;

    public CmdProcess(ILogger logger)
    {
        _logger = logger;
    }

    public CmdExecutionResult ExecuteCommand(string command)
    {
        using var process = new Process();

        ProcessStartInfo startInfo = CreateProcessStartInfo(command);

        _logger.LogTrace("Execute cmd command {command} {arguments}", startInfo.FileName, startInfo.Arguments);

        process.StartInfo = startInfo;
        process.Start();
        // TODO: hack. Without it process will waiting for someone read the stream or write it to parent terminal
        if (!process.WaitForExit((int) _timeout.TotalMilliseconds))
            throw new TimeoutException("Cannot finish cmd operation");

        var standardOutput = process.StandardOutput.ReadToEnd();

        int exitCode = process.ExitCode;
        IReadOnlyCollection<string> errors = GetErrors();
        var cmdExecutionResult = new CmdExecutionResult(standardOutput, exitCode, errors);

        if (cmdExecutionResult.IsAnyError())
            _logger.LogError("Cmd execution finished with exit code {exitCode}.", exitCode);

        process.Close();

        return cmdExecutionResult;
    }

    private ProcessStartInfo CreateProcessStartInfo(string command)
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            return new ProcessStartInfo
            {
                WindowStyle = ProcessWindowStyle.Hidden,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                FileName = "cmd.exe",
                Arguments = $"/C {command}"
            };
        }

        if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            return new ProcessStartInfo
            {
                WindowStyle = ProcessWindowStyle.Hidden,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                FileName = "sh",
                // KB: will not work without ""
                Arguments = $"-c \"{command}\""
            };
        }

        throw new NotSupportedException(RuntimeInformation.OSDescription);
    }

    private IReadOnlyCollection<string> GetErrors()
    {
        var errors = new List<string>();

        // TODO: fixed error stream reading
        // Line splitting triggered by char limit =_=
        //while (!process.StandardError.EndOfStream)
        //{
        //    string? line = process.StandardError.ReadLine();
        //    if (line is not null)
        //        errors.Add(line);
        //}

        return errors;
    }
}