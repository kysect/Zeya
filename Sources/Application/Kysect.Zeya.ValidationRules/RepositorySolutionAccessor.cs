﻿using Kysect.CommonLib.BaseTypes.Extensions;
using Kysect.DotnetSlnParser.Models;
using Kysect.DotnetSlnParser.Parsers;
using Kysect.Zeya.Abstractions;
using Kysect.Zeya.Abstractions.Contracts;
using System.IO.Abstractions;

namespace Kysect.Zeya.ValidationRules;

public class RepositorySolutionAccessor(IClonedRepository repository, SolutionFileParser solutionFileParser, IFileSystem fileSystem)
{
    public string GetSolutionFilePath()
    {
        var repositoryPath = repository.GetFullPath();

        var solutions = fileSystem.Directory.EnumerateFiles(repositoryPath, "*.sln", SearchOption.AllDirectories).ToList();
        if (solutions.Count == 0)
            throw new ZeyaException($"Repository {repositoryPath} does not contains .sln files");

        // TODO: investigate this path
        if (solutions.Count > 1)
            throw new ZeyaException($"Repository {repositoryPath} has more than one solution file.");

        return solutions.Single();
    }

    public string GetSolutionDirectoryPath()
    {
        string solutionFilePath = GetSolutionFilePath();
        IFileInfo fileInfo = fileSystem.FileInfo.New(solutionFilePath);
        fileInfo.Directory.ThrowIfNull();
        return fileInfo.Directory.FullName;
    }

    public IReadOnlyCollection<string> GetProjectPaths()
    {
        string solutionFilePath = GetSolutionFilePath();
        string solutionFileContent = repository.ReadAllText(solutionFilePath);
        IReadOnlyCollection<DotnetProjectFileDescriptor> projectFileDescriptors = solutionFileParser.ParseSolutionFileContent(solutionFileContent);
        string solutionDirectoryPath = GetSolutionDirectoryPath();

        return projectFileDescriptors
            .Select(descriptor => descriptor.ProjectPath)
            .Select(partialPath => fileSystem.Path.Combine(solutionDirectoryPath, partialPath))
            .ToList();
    }

    public string GetDirectoryPackagePropsPath()
    {
        string solutionDirectoryPath = GetSolutionDirectoryPath();
        string fullPath = fileSystem.Path.Combine(solutionDirectoryPath, ValidationConstants.DirectoryPackagePropsFileName);
        return fileSystem.Path.GetRelativePath(repository.GetFullPath(), fullPath);
    }

    public string GetDirectoryBuildPropsPath()
    {
        string solutionDirectoryPath = GetSolutionDirectoryPath();
        string fullPath = fileSystem.Path.Combine(solutionDirectoryPath, ValidationConstants.DirectoryBuildPropsFileName);
        return fileSystem.Path.GetRelativePath(repository.GetFullPath(), fullPath);
    }
}