using Kysect.DotnetProjectSystem.SolutionModification;
using Kysect.Zeya.Common;
using System.IO.Abstractions;

namespace Kysect.Zeya.LocalRepositoryAccess;

public class LocalRepositorySolutionManager(string repositoryPath, string solutionSearchMask, IFileSystem fileSystem, DotnetSolutionModifierFactory solutionModifierFactory)
{
    public const string DefaultMask = "*.sln";

    public LocalRepositorySolution GetSolution()
    {
        string solutionFilePath = GetSolutionFilePath();
        return new LocalRepositorySolution(repositoryPath, solutionFilePath, fileSystem, solutionModifierFactory);
    }

    public string GetSolutionFilePath()
    {
        var solutions = fileSystem.Directory.EnumerateFiles(repositoryPath, solutionSearchMask, SearchOption.AllDirectories).ToList();
        if (solutions.Count == 0)
            throw new ZeyaException($"Repository {repositoryPath} does not contains .sln files");

        if (solutions.Count > 1)
            throw new ZeyaException($"Repository {repositoryPath} has more than one solution file. Specify solution mask for selecting correct solution.");

        return solutions.Single();
    }
}