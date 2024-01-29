namespace Kysect.Zeya.Abstractions.Models;

public record struct RepositoryBranchProtection(bool PullRequestReviewsRequired, bool ConversationResolutionRequired);