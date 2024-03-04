// Copyright 2024 Maintainers of NUKE.
// Distributed under the MIT License.
// https://github.com/nuke-build/nuke/blob/master/LICENSE

using System;
using System.Linq;
using FluentAssertions;
using Nuke.Common.Git;
using Xunit;

namespace Nuke.Common.Tests;

/// <summary>
///     Make assertions about the GitRepositoryAttribute's ability to deal with git working trees.
/// </summary>
/// <remarks>
///     Since GitRepositoryAttribute defers to GitRepository.FromLocalDirectory, we will just test that method
///     directly for the time being.  As an improvement, we should test GitRepositoryAttribute.GetValue(...) instead.
/// </remarks>
public sealed class GitRepositoryWorktreeTests : IClassFixture<GitRepositoryWorktreeFixture>
{
    private readonly Lazy<GitRepository> _testWorkTree;

    private readonly GitRepositoryWorktreeFixture _fixture;

    /// <summary>
    ///     The methods performs the arrange and act steps for the tests.  The Facts perform the assertions.
    /// </summary>
    /// <remarks>
    ///     The setup steps are:
    ///     1.  Determine the git directory for the current test run.
    ///     2.  Create a new working tree at ./git../../nuke-test-worktree with a new branch name (random to avoid possible
    ///     collisions)
    ///     3.  Open a new GitRepositoryAttribute instance of the working tree using GitRepository.FromLocalDirectory(...);
    /// </remarks>
    public GitRepositoryWorktreeTests(GitRepositoryWorktreeFixture fixture)
    {
        _fixture = fixture;
        _testWorkTree = new Lazy<GitRepository>(LoadTestWorkTree);
    }

    private GitRepository TestWorkTree => _testWorkTree.Value;

    [Fact]
    public void RepositoryShouldNotBeNull()
    {
        TestWorkTree.Should().NotBeNull();
    }

    [Fact]
    public void RepositoryShouldHaveTheCorrectBranch()
    {
        TestWorkTree.Branch.Should().Be(_fixture.BranchName);
    }

    [Fact]
    public void RepositoryShouldHaveTheCorrectCommit()
    {
        TestWorkTree.Commit.Should().NotBe(_fixture.MainWorktree.Commit).And.NotBeNull();
    }

    [Fact]
    public void RepositoryShouldHaveTheCorrectEndpoint()
    {
        TestWorkTree.Endpoint.Should().Be(_fixture.MainWorktree.Branch);
    }

    [Fact]
    public void RepositoryShouldHaveTheCorrectHead()
    {
        TestWorkTree.Head.Should().NotBe(_fixture.MainWorktree.Head).And.NotBeNull();
    }

    [Fact]
    public void RepositoryShouldHaveTheCorrectHttpsUrl()
    {
        TestWorkTree.HttpsUrl.Should().Be(_fixture.MainWorktree.HttpsUrl);
    }

    [Fact]
    public void RepositoryShouldHaveTheCorrectIdentifier()
    {
        TestWorkTree.Identifier.Should().Be(_fixture.MainWorktree.Identifier);
    }

    [Fact]
    public void RepositoryShouldHaveTheCorrectLocalDirectory()
    {
        TestWorkTree.LocalDirectory.Should().Be(_fixture.WorktreeDirectory);
    }

    [Fact]
    public void RepositoryShouldHaveTheCorrectProtocol()
    {
        TestWorkTree.Protocol.Should().Be(_fixture.MainWorktree.Protocol);
    }

    [Fact]
    public void RepositoryShouldHaveTheCorrectRemoteBranch()
    {
        TestWorkTree.RemoteBranch.Should().BeNull();
    }

    [Fact]
    public void RepositoryShouldHaveTheCorrectRemoteName()
    {
        TestWorkTree.RemoteName.Should().BeNull();
    }

    [Fact]
    public void RepositoryShouldHaveTheCorrectSshUrl()
    {
        TestWorkTree.SshUrl.Should().Be(_fixture.MainWorktree.SshUrl);
    }

    [Fact]
    public void RepositoryShouldHaveTheCorrectTags()
    {
        TestWorkTree.Tags.Should().Equal(_fixture.MainWorktree.Tags);
    }

    private GitRepository LoadTestWorkTree()
    {
        return GitRepository.FromLocalDirectory(_fixture.WorktreeDirectory).NotNull();
    }
}
