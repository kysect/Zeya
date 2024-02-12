﻿using Kysect.Zeya.LocalRepositoryAccess.Github;
using Spectre.Console;

namespace Kysect.Zeya.Tui.Controls;

public static class RepositoryNameInputControl
{
    public static GithubRepositoryName Ask()
    {
        string repositoryFullName = AnsiConsole.Ask<string>("Repository (format: org/repo):");
        if (!repositoryFullName.Contains('/'))
            throw new ArgumentException("Incorrect repository format");

        string[] parts = repositoryFullName.Split('/', 2);
        return new GithubRepositoryName(parts[0], parts[1]);
    }
}