namespace Kysect.Zeya.ValidationRules;

public static class RuleDescription
{
    public static class Github
    {
        public static string RepositoryLicense = "GHR0001";
        public static string ReadmeExists = "GHR0002";
        public static string BranchProtectionEnabled = "GHR0003";
        public static string AutoBranchDeletionEnabled = "GHR0003";
        public static string BuildWorkflowEnabled = "GHR0004";
    }

    public static class SourceCode
    {
        public static string SourcesMustNotBeInRoot = "SRC00001";
        public static string TargetFrameworkVersionAllowed = "SRC00002";
        public static string RequiredPackagesAdded = "SRC0005";
        public static string ArtifactsOutputEnables = "SRC0006";

        public static string CentralPackageManagerEnabled = "SRC0010";
        public static string NugetVersionMustBeSpecifiedInMasterCentralPackageManager = "SRC0011";
        public static string NugetVersionSynchronizedWithMasterCentralPackageManager = "SRC0012";

    }

    public static class Nuget
    {
        public static string MetadataSpecified = "NUP0001";
    }
}