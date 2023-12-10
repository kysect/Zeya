using System.IO.Abstractions;
using Kysect.DotnetSlnParser.Modifiers;
using Kysect.DotnetSlnParser.Parsers;
using Microsoft.Extensions.Logging;

namespace Kysect.Zeya.ProjectSystemIntegration;

public class DotnetSolutionModifierFactory(IFileSystem fileSystem, ILogger logger)
{
    public DotnetSolutionModifier Create(string solutionPath)
    {
        return DotnetSolutionModifier.Create(solutionPath, fileSystem, logger, new SolutionFileParser(logger));

    }
}