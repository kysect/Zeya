﻿namespace Kysect.Zeya.GithubIntegration.Abstraction.Models;

public record struct RepositoryBranchProtection(bool PullRequestReviewsRequired, bool ConversationResolutionRequired);