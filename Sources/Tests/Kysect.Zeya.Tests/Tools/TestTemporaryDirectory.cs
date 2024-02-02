using Kysect.CommonLib.BaseTypes.Extensions;
using Kysect.CommonLib.FileSystem;
using System.IO.Abstractions;

namespace Kysect.Zeya.Tests.Tools;

public class TestTemporaryDirectory : IDisposable
{
    private readonly IFileSystem _fileSystem;
    private readonly IDirectoryInfo _directoryInfo;

    public TestTemporaryDirectory(IFileSystem fileSystem, string path)
    {
        path.ThrowIfNull();
        _fileSystem = fileSystem.ThrowIfNull();

        if (_fileSystem.Directory.Exists(path))
        {
            IDirectoryInfo directoryInfo = _fileSystem.DirectoryInfo.New(path);
            DeleteRecursive(directoryInfo);
        }

        _directoryInfo = _fileSystem.Directory.CreateDirectory(path);
        DirectoryExtensions.EnsureDirectoryExists(fileSystem, path);
    }

    public void Dispose()
    {
        DeleteRecursive(_directoryInfo);
    }

    // KB: https://github.com/libgit2/libgit2sharp/issues/1354
    public static void DeleteRecursive(IDirectoryInfo target)
    {
        target.ThrowIfNull();

        if (!target.Exists)
        {
            return;
        }

        foreach (var file in target.EnumerateFiles())
        {
            if (file.IsReadOnly)
            {
                file.IsReadOnly = false;
            }

            file.Delete();
        }

        foreach (IDirectoryInfo dir in target.EnumerateDirectories())
        {
            DeleteRecursive(dir);
        }

        target.Delete();
    }
}