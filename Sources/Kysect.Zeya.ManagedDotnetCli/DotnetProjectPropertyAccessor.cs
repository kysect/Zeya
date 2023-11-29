namespace Kysect.Zeya.ManagedDotnetCli;

public class DotnetProjectPropertyAccessor
{
    private readonly string _projectPath;
    private readonly DotnetCli _cli;

    public DotnetProjectPropertyAccessor(string projectPath, DotnetCli cli)
    {
        _projectPath = projectPath;
        _cli = cli;
    }

    public bool ManagePackageVersionsCentrally()
    {
        return GetBoolValue("ManagePackageVersionsCentrally");
    }

    public bool IsTestProject()
    {
        return GetBoolValue("IsTestProject");
    }

    private bool GetBoolValue(string propertyName)
    {
        var property = _cli.GetProperty(_projectPath, propertyName);

        if (string.IsNullOrWhiteSpace(property))
            return false;

        if (bool.TryParse(property, out var result))
        {
            return result;
        }

        throw new DotnetCliException($"Cannot parse {property} as bool");
    }
}