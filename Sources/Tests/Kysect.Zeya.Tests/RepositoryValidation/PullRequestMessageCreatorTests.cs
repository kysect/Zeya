﻿using FluentAssertions;
using Kysect.Zeya.Abstractions;
using Kysect.Zeya.RepositoryValidation;
using Kysect.Zeya.ValidationRules.Rules.SourceCode;

namespace Kysect.Zeya.Tests.RepositoryValidation;

public class PullRequestMessageCreatorTests
{
    private readonly PullRequestMessageCreator _pullRequestMessageCreator;

    public PullRequestMessageCreatorTests()
    {
        _pullRequestMessageCreator = new PullRequestMessageCreator();
    }

    [Fact]
    public void Create_EmptyFixList_ThrowException()
    {
        ZeyaException exception = Assert.Throws<ZeyaException>(() =>
        {
            _pullRequestMessageCreator.Create([]);
        });

        exception.Message.Should().Be("Cannot create message for pull request. No fixed rules.");
    }

    [Fact]
    public void Create_OneFix_ReturnExpectedString()
    {
        var expected = """
                       Fixed problems:

                       - SRC0010
                       
                       """;

        string message = _pullRequestMessageCreator.Create([new CentralPackageManagerEnabledValidationRule.Arguments()]);

        message.Should().Be(expected);
    }

    [Fact]
    public void Create_TwoFix_ReturnExpectedString()
    {
        var expected = """
                       Fixed problems:

                       - SRC0010
                       - SRC0003

                       """;

        string message = _pullRequestMessageCreator.Create(
        [
            new CentralPackageManagerEnabledValidationRule.Arguments(),
            new ArtifactsOutputEnabledValidationRule.Arguments(),
        ]);

        message.Should().Be(expected);
    }
}