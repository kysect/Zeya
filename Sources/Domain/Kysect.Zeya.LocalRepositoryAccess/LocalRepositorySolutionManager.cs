using Kysect.Zeya.Common;
using System.IO.Abstractions;

namespace Kysect.Zeya.LocalRepositoryAccess;

public class LocalRepositorySolutionManager(string repositoryPath, IFileSystem fileSystem)
{
    public LocalRepositorySolution GetSolution()
    {
        string solutionFilePath = GetSolutionFilePath();
        return new LocalRepositorySolution(repositoryPath, solutionFilePath, fileSystem);
    }

    private string GetSolutionFilePath()
    {
        var solutions = fileSystem.Directory.EnumerateFiles(repositoryPath, "*.sln", SearchOption.AllDirectories).ToList();
        if (solutions.Count == 0)
            throw new ZeyaException($"Repository {repositoryPath} does not contains .sln files");

        // TODO: investigate this path
        if (solutions.Count > 1)
            throw new ZeyaException($"Repository {repositoryPath} has more than one solution file.");

        return solutions.Single();
    }
}