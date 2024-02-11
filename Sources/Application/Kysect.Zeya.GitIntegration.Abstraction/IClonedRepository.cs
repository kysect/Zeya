namespace Kysect.Zeya.GitIntegration.Abstraction;

public interface IClonedRepository
{
    string GetRepositoryName();

    LocalRepositoryFileSystem FileSystem { get; }
}
