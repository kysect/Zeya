using Kysect.CommonLib.BaseTypes.Extensions;
using Kysect.DotnetProjectSystem.Tools;
using Kysect.Zeya.LocalRepositoryAccess;
using Microsoft.Extensions.Logging;

namespace Kysect.Zeya.RepositoryValidation.ProcessingActions.Fix;

public record RepositoryFixProcessingActionRequest(IReadOnlyCollection<IValidationRule> Rules, IReadOnlyCollection<string> ValidationRuleCodeForFix);
public record RepositoryFixProcessingActionResponse(IReadOnlyCollection<IValidationRule> FixedRules);

public class RepositoryFixProcessingAction(IValidationRuleFixerApplier validationRuleFixerApplier, ILogger<RepositoryFixProcessingAction> logger)
    : IRepositoryProcessingAction<RepositoryFixProcessingActionRequest, RepositoryFixProcessingActionResponse>
{
    public RepositoryProcessingResponse<RepositoryFixProcessingActionResponse> Process(ILocalRepository repository, RepositoryFixProcessingActionRequest request)
    {
        repository.ThrowIfNull();
        request.ThrowIfNull();

        logger.LogInformation("Run fixer for {Repository}", repository.GetRepositoryName());

        var fixedRules = new List<IValidationRule>();
        var validationRules = request.Rules.ToDictionary(r => r.DiagnosticCode, r => r);

        foreach (string code in request.ValidationRuleCodeForFix)
        {
            if (!validationRules.TryGetValue(code, out IValidationRule? validationRule))
                throw new DotnetProjectSystemException($"Rule {code} was not found");


            if (validationRuleFixerApplier.IsFixerRegistered(validationRule))
            {
                logger.LogInformation("Apply code fixer for {Code}", code);
                validationRuleFixerApplier.Apply(validationRule, repository);
                fixedRules.Add(validationRule);
            }
            else
            {
                logger.LogDebug("Fixer for {Code} is not available", code);
            }
        }

        // TODO: Return info about fixed rules
        return new RepositoryProcessingResponse<RepositoryFixProcessingActionResponse>("Fix diagnostics", new RepositoryFixProcessingActionResponse(fixedRules), []);
    }
}