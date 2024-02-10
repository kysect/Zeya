using Kysect.DotnetProjectSystem.Parsing;
using Kysect.Zeya.GitIntegration.Abstraction;
using System.IO.Abstractions;

namespace Kysect.Zeya.GitIntegration;

public class RepositorySolutionAccessorFactory(SolutionFileContentParser solutionFileParser, IFileSystem fileSystem)
{
    public RepositorySolutionAccessor Create(IClonedRepository repository)
    {
        return new RepositorySolutionAccessor(repository, solutionFileParser, fileSystem);
    }
}