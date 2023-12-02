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
    }
}