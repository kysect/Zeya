using Kysect.CommonLib.BaseTypes.Extensions;
using Kysect.DotnetProjectSystem.Parsing;
using Kysect.Zeya.LocalRepositoryAccess;
using System.Collections.Generic;
using System.Linq;

namespace Kysect.Zeya.RepositoryDependencies.NuGetPackagesSearcher;

public class NuGetPackagesSearcher(SolutionFileContentParser solutionFileContentParser) : INuGetPackagesSearcher
{
    public IReadOnlyCollection<string> GetContainingProjectNames(ILocalRepository repository)
    {
        repository.ThrowIfNull();

        string solutionFilePath = repository.SolutionManager.GetSolutionFilePath();
        string solutionFileContent = repository.FileSystem.ReadAllText(solutionFilePath);
        IReadOnlyCollection<DotnetProjectFileDescriptor> projectFileDescriptors = solutionFileContentParser.ParseSolutionFileContent(solutionFileContent);
        var nugetPackages = projectFileDescriptors.Select(p => p.ProjectName).ToList();

        return nugetPackages;
    }
}