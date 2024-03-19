namespace Kysect.Zeya.RepositoryValidationRules.Rules;

public static class RuleDescription
{
#pragma warning disable CA1724
    public static class Github
    {
        public const string RepositoryLicense = "GHR0001";
        public const string ReadmeExists = "GHR0002";
        public const string BranchProtectionEnabled = "GHR0003";
        public const string AutoBranchDeletionEnabled = "GHR0004";
        public const string BuildWorkflowEnabled = "GHR0005";
    }

    public static class SourceCode
    {
        public const string SourcesMustNotBeInRoot = "SRC00001";
        public const string TargetFrameworkVersionAllowed = "SRC00002";
        public const string ArtifactsOutputEnables = "SRC0003";
        public const string VersionInPropFile = "SRC0004";
        public const string SolutionStructureMatchFile = "SRC0005";

        public const string CentralPackageManagerEnabled = "SRC0010";
        public const string NugetVersionMustBeSpecifiedInMasterCentralPackageManager = "SRC0011";
        public const string NugetVersionSynchronizedWithMasterCentralPackageManager = "SRC0012";
        public const string RequiredPackagesAdded = "SRC0013";

    }

    public static class Nuget
    {
        public const string MetadataSpecified = "NUP0001";
        public const string MetadataHaveCorrectValue = "NUP0002";
    }
#pragma warning restore CA1724
}