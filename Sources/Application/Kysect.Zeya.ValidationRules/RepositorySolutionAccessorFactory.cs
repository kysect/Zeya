using Kysect.DotnetSlnParser.Parsers;
using Kysect.Zeya.Abstractions.Contracts;
using System.IO.Abstractions;

namespace Kysect.Zeya.ValidationRules;

public class RepositorySolutionAccessorFactory(SolutionFileParser solutionFileParser, IFileSystem fileSystem)
{
    public RepositorySolutionAccessor Create(IClonedRepository repository)
    {
        return new RepositorySolutionAccessor(repository, solutionFileParser, fileSystem);
    }
}