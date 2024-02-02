using Kysect.DotnetProjectSystem.Parsing;
using Kysect.Zeya.Abstractions.Contracts;
using System.IO.Abstractions;

namespace Kysect.Zeya.RepositoryAccess;

public class RepositorySolutionAccessorFactory(SolutionFileContentParser solutionFileParser, IFileSystem fileSystem)
{
    public RepositorySolutionAccessor Create(IClonedRepository repository)
    {
        return new RepositorySolutionAccessor(repository, solutionFileParser, fileSystem);
    }
}