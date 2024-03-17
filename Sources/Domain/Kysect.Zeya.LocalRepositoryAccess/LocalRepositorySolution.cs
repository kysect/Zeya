using Kysect.CommonLib.BaseTypes.Extensions;
using Kysect.DotnetProjectSystem.SolutionModification;
using Kysect.DotnetProjectSystem.Tools;
using System.IO.Abstractions;

namespace Kysect.Zeya.LocalRepositoryAccess;

public class LocalRepositorySolution(string repositoryPath, string solutionFilePath, IFileSystem fileSystem, DotnetSolutionModifierFactory solutionModifierFactory)
{
    public DotnetSolutionModifier GetSolutionModifier()
    {
        return solutionModifierFactory.Create(solutionFilePath);
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