using Kysect.CommonLib.BaseTypes.Extensions;
using Kysect.Zeya.Abstractions.Contracts;
using System.Collections.Generic;
using System.Text;

namespace Kysect.Zeya.RepositoryValidation;

public class PullRequestMessageCreator
{
    public string Create(IReadOnlyCollection<IValidationRule> fixedDiagnostics)
    {
        fixedDiagnostics.ThrowIfNull();

        var sb = new StringBuilder();

        sb.AppendLine("Fixed problems:")
            .AppendLine();

        foreach (IValidationRule fixedDiagnostic in fixedDiagnostics)
            sb.AppendLine($"- {fixedDiagnostic.DiagnosticCode}");

        return sb.ToString();
    }
}