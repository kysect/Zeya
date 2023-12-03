﻿using Kysect.Zeya.Abstractions.Contracts;

namespace Kysect.Zeya.ValidationRules;

public record RepositoryValidationContext(IGithubRepositoryAccessor RepositoryAccessor, RepositoryDiagnosticCollector DiagnosticCollector);