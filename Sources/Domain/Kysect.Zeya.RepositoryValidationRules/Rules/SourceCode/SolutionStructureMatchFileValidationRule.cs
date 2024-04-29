using Kysect.CommonLib.BaseTypes.Extensions;
using Kysect.DotnetProjectSystem.Parsing;
using Kysect.DotnetProjectSystem.SolutionModification;
using Kysect.ScenarioLib.Abstractions;
using Kysect.Zeya.LocalRepositoryAccess;
using Kysect.Zeya.RepositoryValidation;
using Kysect.Zeya.RepositoryValidation.ProcessingActions.Validation;
using System.IO.Abstractions;

namespace Kysect.Zeya.RepositoryValidationRules.Rules.SourceCode;

public class SolutionStructureMatchFileValidationRule(SolutionFileContentParser solutionFileContentParser, IFileSystem fileSystem)
    : IScenarioStepExecutor<SolutionStructureMatchFileValidationRule.Arguments>
{
    [ScenarioStep("SourceCode.SolutionStructureMatchFile")]
    public record Arguments() : IValidationRule
    {
        public string DiagnosticCode => RuleDescription.SourceCode.SolutionStructureMatchFile;
        public const RepositoryValidationSeverity DefaultSeverity = RepositoryValidationSeverity.Warning;
    }

    public void Execute(ScenarioContext context, Arguments request)
    {
        context.ThrowIfNull();
        request.ThrowIfNull();

        RepositoryValidationContext repositoryValidationContext = context.GetValidationContext();
        LocalRepositorySolution localRepositorySolution = repositoryValidationContext.Repository.SolutionManager.GetSolution();
        DotnetSolutionModifier dotnetSolutionModifier = localRepositorySolution.GetSolutionModifier();

        string solutionFilePath = repositoryValidationContext.Repository.SolutionManager.GetSolutionFilePath();
        string solutionFileContent = repositoryValidationContext.Repository.FileSystem.ReadAllText(solutionFilePath);

        IReadOnlyCollection<DotnetProjectFileDescriptor> projects = solutionFileContentParser.ParseSolutionFileContent(solutionFileContent);
        foreach (DotnetProjectFileDescriptor project in projects)
        {
            string projectSolutionPath = fileSystem.Path.Combine(project.SolutionStructurePath, $"{project.ProjectName}.csproj");
            if (project.FileSystemPath != projectSolutionPath)
            {
                repositoryValidationContext.DiagnosticCollector.AddDiagnostic(
                    request.DiagnosticCode,
                    $"Project name does not match with directory name. Project file system path: {project.FileSystemPath}, solution structure path: {projectSolutionPath}",
                    Arguments.DefaultSeverity);
            }
        }
    }
}