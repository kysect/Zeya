﻿namespace Kysect.Zeya.GithubIntegration.Abstraction;

public record struct RepositoryBranchProtection(bool PullRequestReviewsRequired, bool ConversationResolutionRequired);