using Kysect.CommonLib.BaseTypes.Extensions;
using Kysect.DotnetProjectSystem.Tools;
using Kysect.Zeya.LocalRepositoryAccess;
using Microsoft.Extensions.Logging;

namespace Kysect.Zeya.RepositoryValidation.ProcessingActions;

public class RepositoryFixProcessingAction(IValidationRuleFixerApplier validationRuleFixerApplier, ILogger<RepositoryFixProcessingAction> logger)
    : IRepositoryProcessingAction<RepositoryFixProcessingAction.Request, RepositoryFixProcessingAction.Response>
{
    public record Request(IReadOnlyCollection<IValidationRule> Rules, IReadOnlyCollection<string> ValidationRuleCodeForFix);
    public record Response(IReadOnlyCollection<IValidationRule> FixedRules);

    public Response Process(ILocalRepository repository, Request request)
    {
        repository.ThrowIfNull();
        request.ThrowIfNull();

        // TODO: log fix result
        logger.LogInformation("Run fixer for {Repository}", repository.GetRepositoryName());

        var fixedRules = new List<IValidationRule>();
        Dictionary<string, IValidationRule> validationRules = request.Rules.ToDictionary(r => r.DiagnosticCode, r => r);

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

        return new Response(fixedRules);
    }
}