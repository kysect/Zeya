using Kysect.CommonLib.BaseTypes.Extensions;
using Kysect.DotnetProjectSystem.Parsing;
using Kysect.DotnetProjectSystem.SolutionModification;
using Kysect.DotnetProjectSystem.Tools;
using System.IO.Abstractions;

namespace Kysect.Zeya.GitIntegration.Abstraction;

public class LocalRepositorySolution(string repositoryPath, string solutionFilePath, IFileSystem fileSystem)
{
    private readonly DotnetSolutionModifierFactory _solutionModifierFactory = new DotnetSolutionModifierFactory(fileSystem, new SolutionFileContentParser());

    public DotnetSolutionModifier GetSolutionModifier()
    {
        return _solutionModifierFactory.Create(solutionFilePath);
    }

    public string GetSolutionDirectoryPath()
    {
        IFileInfo fileInfo = fileSystem.FileInfo.New(solutionFilePath);
        fileInfo.Directory.ThrowIfNull();
        return fileInfo.Directory.FullName;
    }

    public string GetDirectoryPackagePropsPath()
    {
        string solutionDirectoryPath = GetSolutionDirectoryPath();
        string fullPath = fileSystem.Path.Combine(solutionDirectoryPath, SolutionItemNameConstants.DirectoryPackagesProps);
        return fileSystem.Path.GetRelativePath(repositoryPath, fullPath);
    }

    public string GetDirectoryBuildPropsPath()
    {
        string solutionDirectoryPath = GetSolutionDirectoryPath();
        string fullPath = fileSystem.Path.Combine(solutionDirectoryPath, SolutionItemNameConstants.DirectoryBuildProps);
        return fileSystem.Path.GetRelativePath(repositoryPath, fullPath);
    }
}