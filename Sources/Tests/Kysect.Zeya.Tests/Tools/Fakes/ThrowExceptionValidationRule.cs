using Kysect.ScenarioLib.Abstractions;
using Kysect.Zeya.Common;
using Kysect.Zeya.RepositoryValidation;

namespace Kysect.Zeya.Tests.Tools.Fakes;

public class ThrowExceptionValidationRule() : IScenarioStepExecutor<ThrowExceptionValidationRule.Arguments>
{
    public const string Message = "Some test exception";

    [ScenarioStep("ThrowExceptionValidationRule")]
    public record Arguments() : IValidationRule
    {
        public string DiagnosticCode => "CODE0002";
        public const RepositoryValidationSeverity DefaultSeverity = RepositoryValidationSeverity.Warning;
    }

    public void Execute(ScenarioContext context, Arguments request)
    {
        throw new ZeyaException(Message);
    }
}