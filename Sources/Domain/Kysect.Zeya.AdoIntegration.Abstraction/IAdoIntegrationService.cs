﻿namespace Kysect.Zeya.AdoIntegration.Abstraction;

public interface IAdoIntegrationService
{
    Task<bool> BuildValidationEnabled(AdoRepositoryUrl repositoryUrl);
}