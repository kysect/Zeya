﻿using Kysect.CommonLib.BaseTypes.Extensions;
using Kysect.CommonLib.Collections.Extensions;
using Kysect.Zeya.Common;
using System.Text;

namespace Kysect.Zeya.RepositoryValidation.ProcessingActions.CreatePullRequest;

public class PullRequestMessageCreator
{
    public string Create(IReadOnlyCollection<IValidationRule> fixedDiagnostics)
    {
        fixedDiagnostics.ThrowIfNull();

        if (fixedDiagnostics.IsEmpty())
            throw new ZeyaException("Cannot create message for pull request. No fixed rules.");

        var sb = new StringBuilder();

        sb.AppendLine("Fixed problems:")
            .AppendLine();

        foreach (IValidationRule fixedDiagnostic in fixedDiagnostics)
            sb.AppendLine($"- {fixedDiagnostic.DiagnosticCode}");

        return sb.ToString();
    }
}