using Kysect.CommonLib.Logging;
using Kysect.Zeya.Abstractions.Contracts;
using Kysect.Zeya.Abstractions.Models;
using Microsoft.Extensions.Logging;

namespace Kysect.Zeya;

public class RepositoryValidator
{
    private readonly ILogger _logger;

    public RepositoryValidator(ILogger logger)
    {
        _logger = logger;
    }
    public RepositoryValidationReport Validate(GithubRepository repository, IReadOnlyCollection<IRepositoryValidationRule<GithubRepository>> rule)
    {
        var result = RepositoryValidationReport.Empty;

        _logger.LogInformation("Validate repository {Url}", repository.FullName);
        foreach (var validationRule in rule)
        {
            _logger.LogTabDebug(1, $"Validate via rule {validationRule.Name}");
            var validationResult = validationRule.Validate(repository);
            result = result.Compose(validationResult);
        }

        return result;
    }
}