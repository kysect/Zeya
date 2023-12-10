namespace Kysect.Zeya.ManagedDotnetCli;

public class DotnetProjectPropertyAccessor(DotnetCli cli) : IDotnetProjectPropertyAccessor
{
    public bool IsManagePackageVersionsCentrally(string projectPath)
    {
        return GetBoolValue(projectPath, "ManagePackageVersionsCentrally");
    }

    public bool IsTestProject(string projectPath)
    {
        return GetBoolValue(projectPath, "IsTestProject");
    }

    public string GetTargetFramework(string projectPath)
    {
        // TODO: move to constants
        return cli.GetProperty(projectPath, "TargetFramework");
    }

    private bool GetBoolValue(string projectPath, string propertyName)
    {
        var property = cli.GetProperty(projectPath, propertyName);

        if (string.IsNullOrWhiteSpace(property))
            return false;

        if (bool.TryParse(property, out var result))
        {
            return result;
        }

        throw new DotnetCliException($"Cannot parse {property} as bool");
    }
}