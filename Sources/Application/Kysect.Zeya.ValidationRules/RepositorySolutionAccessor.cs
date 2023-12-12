using Kysect.DotnetSlnParser.Parsers;
using Kysect.Zeya.Abstractions;
using Kysect.Zeya.Abstractions.Contracts;

namespace Kysect.Zeya.ValidationRules;

public class RepositorySolutionAccessor(IGithubRepositoryAccessor repositoryAccessor, SolutionFileParser solutionFileParser)
{
    public IReadOnlyCollection<string> GetProjectPaths()
    {
        var solutionFilePath = repositoryAccessor.GetSolutionFilePath();
        var solutionFileContent = repositoryAccessor.ReadFile(solutionFilePath);
        var projectFileDescriptors = solutionFileParser.ParseSolutionFileContent(solutionFileContent);

        // TODO: use IFileSystem
        var solutionDirectory = new FileInfo(solutionFilePath).Directory;
        if (solutionDirectory is null)
            throw new ZeyaException($"Cannot get solution directory for {solutionFilePath}");

        // TODO: use IFileSystem
        return projectFileDescriptors
            .Select(descriptor => descriptor.ProjectPath)
            .Select(partialPath => Path.Combine(solutionDirectory.FullName, partialPath))
            .ToList();
    }
}