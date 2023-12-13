namespace Kysect.Zeya.ValidationRules;

public static class ValidationConstants
{
    public const string LicenseFileName = "LICENSE";
    public const string ReadmeFileName = "Readme.md";
    public const string DefaultBranch = "master";
    public const string DefaultSourceCodeDirectory = "Sources";
    public const string DirectoryPackagePropsFileName = "Directory.Packages.props";
    public const string DirectoryBuildPropsFileName = "Directory.Build.props";

    // TODO: remove this
    public static string DirectoryPackagePropsPath { get; } = Path.Combine(DefaultSourceCodeDirectory, DirectoryPackagePropsFileName);
    public static string DirectoryBuildPropsPath { get; } = Path.Combine(DefaultSourceCodeDirectory, DirectoryBuildPropsFileName);
}