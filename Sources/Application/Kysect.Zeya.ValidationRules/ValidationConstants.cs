﻿namespace Kysect.Zeya.ValidationRules;

public static class ValidationConstants
{
    public static string LicenseFileName = "LICENSE";
    public static string ReadmeFileName = "Readme.md";
    public static string DefaultBranch = "master";
    public static string DefaultSourceCodeDirectory = "Sources";
    public static string DirectoryPackagePropsFileName = "Directory.Packages.props";
    public static string DirectoryBuildPropsFileName = "Directory.Build.props";
    public static string DirectoryPackagePropsPath = Path.Combine(DefaultSourceCodeDirectory, DirectoryPackagePropsFileName);
    public static string DirectoryBuildPropsPath = Path.Combine(DefaultSourceCodeDirectory, DirectoryBuildPropsFileName);
}