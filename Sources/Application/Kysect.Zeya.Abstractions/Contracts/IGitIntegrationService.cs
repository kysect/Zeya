﻿namespace Kysect.Zeya.Abstractions.Contracts;

public interface IGitIntegrationService
{
    void CreateFixBranch(IClonedRepository repository, string branchName);
    void CreateCommitWithFix(IClonedRepository repository, string commitMessage);
    void PushCommitToRemote(IClonedRepository repository, string branchName);
}