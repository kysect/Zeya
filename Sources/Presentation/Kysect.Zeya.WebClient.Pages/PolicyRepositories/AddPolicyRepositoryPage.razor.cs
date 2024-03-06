﻿using Kysect.Zeya.Client.Abstractions;
using Microsoft.AspNetCore.Components;
using System;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;

namespace Kysect.Zeya.WebClient.Pages.PolicyRepositories;

public partial class AddPolicyRepositoryPage
{
    public class AddPolicyGithubRepositoryForm
    {
        [Required]
        public string GithubOwner { get; set; } = string.Empty;
        [Required]
        public string GithubRepository { get; set; } = string.Empty;
    }

    [Parameter] public Guid PolicyId { get; set; }
    [Inject] private NavigationManager _navigationManager { get; set; } = null!;
    [Inject] private IPolicyRepositoryService PolicyRepositoryService { get; set; } = null!;

    private readonly AddPolicyGithubRepositoryForm _formData = new AddPolicyGithubRepositoryForm();

    private async Task AddPolicyRepository()
    {
        await PolicyRepositoryService.AddGithubRepository(PolicyId, _formData.GithubOwner, _formData.GithubRepository);
        _navigationManager.NavigateTo($"/validation-policies/{PolicyId}");
    }
}