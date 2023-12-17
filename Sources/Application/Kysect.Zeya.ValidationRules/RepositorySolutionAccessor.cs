using Kysect.DotnetSlnParser.Parsers;
using Kysect.Zeya.Abstractions;
using Kysect.Zeya.Abstractions.Contracts;
using System.IO.Abstractions;

namespace Kysect.Zeya.ValidationRules;

public class RepositorySolutionAccessor(IGithubRepositoryAccessor repositoryAccessor, SolutionFileParser solutionFileParser, IFileSystem fileSystem)
{
    public string GetSolutionFilePath()
    {
        var repositoryPath = repositoryAccessor.GetFullPath();

        var solutions = fileSystem.Directory.EnumerateFiles(repositoryPath, "*.sln", SearchOption.AllDirectories).ToList();
        if (solutions.Count == 0)
            throw new ZeyaException($"Repository {repositoryPath} does not contains .sln files");

        // TODO: investigate this path
        if (solutions.Count > 1)
            throw new ZeyaException($"Repository {repositoryPath} has more than one solution file.");

        return solutions.Single();
    }

    public IReadOnlyCollection<string> GetProjectPaths()
    {
        var solutionFilePath = repositoryAccessor.GetSolutionFilePath();
        var solutionFileContent = repositoryAccessor.ReadAllText(solutionFilePath);
        var projectFileDescriptors = solutionFileParser.ParseSolutionFileContent(solutionFileContent);

        // TODO: use IFileSystem
        var solutionDirectory = fileSystem.FileInfo.New(solutionFilePath).Directory;
        if (solutionDirectory is null)
            throw new ZeyaException($"Cannot get solution directory for {solutionFilePath}");

        // TODO: use IFileSystem
        return projectFileDescriptors
            .Select(descriptor => descriptor.ProjectPath)
            .Select(partialPath => fileSystem.Path.Combine(solutionDirectory.FullName, partialPath))
            .ToList();
    }

    public string GetDirectoryPackagePropsPath()
    {
        string solutionFilePath = GetSolutionFilePath();
        string fullPath = fileSystem.Path.Combine(solutionFilePath, ValidationConstants.DirectoryPackagePropsFileName);
        return fileSystem.Path.GetRelativePath(repositoryAccessor.GetFullPath(), fullPath);
    }

    public string GetDirectoryBuildPropsPath()
    {
        string solutionFilePath = GetSolutionFilePath();
        string fullPath = fileSystem.Path.Combine(solutionFilePath, ValidationConstants.DirectoryBuildPropsFileName);
        return fileSystem.Path.GetRelativePath(repositoryAccessor.GetFullPath(), fullPath);
    }
}