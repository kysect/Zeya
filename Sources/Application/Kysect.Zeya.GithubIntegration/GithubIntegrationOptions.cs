﻿using Kysect.Zeya.GitIntegration;
using System.ComponentModel.DataAnnotations;

namespace Kysect.Zeya.GithubIntegration;

public class GithubIntegrationOptions
{
    public GitCommitAuthor? CommitAuthor { get; init; }

    [Required]
    public string GithubUsername { get; init; } = null!;
    [Required]
    public string GithubToken { get; init; } = null!;
    [Required]
    public string CacheDirectoryPath { get; init; } = null!;
}