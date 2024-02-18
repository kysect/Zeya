﻿using Kysect.Zeya.GithubIntegration.Abstraction;
using Spectre.Console;

namespace Kysect.Zeya.Tui.Controls;

public class RepositoryNameInputControl(IAnsiConsole console)
{
    public GithubRepositoryName Ask()
    {
        string repositoryFullName = console.Ask<string>("Repository (format: org/repo):");
        if (!repositoryFullName.Contains('/'))
            throw new ArgumentException("Incorrect repository format");

        string[] parts = repositoryFullName.Split('/', 2);
        return new GithubRepositoryName(parts[0], parts[1]);
    }
}