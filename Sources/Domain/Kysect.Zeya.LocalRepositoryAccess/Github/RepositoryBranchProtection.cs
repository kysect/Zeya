﻿namespace Kysect.Zeya.LocalRepositoryAccess.Github;

public record struct RepositoryBranchProtection(bool PullRequestReviewsRequired, bool ConversationResolutionRequired);