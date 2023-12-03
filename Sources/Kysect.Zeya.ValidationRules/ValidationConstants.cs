namespace Kysect.Zeya.ValidationRules;

public static class ValidationConstants
{
    public static string LicenseFileName = "LICENSE";
    public static string ReadmeFileName = "ReadmeExists.md";
    public static string DefaultBranch = "master";
    public static string DefaultSourceCodeDirectory = "Sources";
    public static string DirectoryPackagePropsFileName = "Directory.Packages.props";
    public static string DirectoryPackagePropsPath = Path.Combine(DefaultSourceCodeDirectory, DirectoryPackagePropsFileName);
}