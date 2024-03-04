// Copyright 2024 Maintainers of NUKE.
// Distributed under the MIT License.
// https://github.com/nuke-build/nuke/blob/master/LICENSE

using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Nuke.Common.Git;
using Nuke.Common.IO;

namespace Nuke.Common.Tests;

public sealed class GitRepositoryWorktreeFixture : IDisposable
{
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
    public GitRepositoryWorktreeFixture()
    {
        // This is the build's current directory.
        var currentDirectory = Directory.GetCurrentDirectory();

        // Use the GitRepository class to get the root of the git repository.  We need this because our worktree needs to live outside the main
        // working tree.
        MainWorktree = GitRepository.FromLocalDirectory(currentDirectory).NotNull();
        var gitDirectory = MainWorktree.LocalDirectory;
        var gitDirectoryName = gitDirectory.Name;

        // This is where we will create our test working tree.  This is a field because we need to clean it up.
        WorktreeDirectory = gitDirectory / .. / $"{gitDirectoryName}-test-worktree";

        // Remove the working tree destination if it exists.  It shouldn't but this will make things run more smoothly.
        DeleteTestWorktreeDirectory();

        // Prune any orphaned working trees.
        PruneOrphanedWorktrees();

        // This is the new branch name we'll use.  This is a field because we need to clean it up.
        var branchGuid = Guid.NewGuid().ToString().Replace("-", string.Empty);
        BranchName = $"test-worktree-{branchGuid}";

        // Create the test worktree.
        CreateTestWorktree();
    }

    ~GitRepositoryWorktreeFixture()
    {
        DisposeInternal();
    }

    public void Dispose()
    {
        DisposeInternal();
        GC.SuppressFinalize(this);
    }

    internal string BranchName { get; }

    internal GitRepository MainWorktree { get; }

    internal AbsolutePath WorktreeDirectory { get; }

    private void CreateTestWorktree()
    {
        var createWorktree = new Process { StartInfo = ConfigureProcessStartInfo("git.exe", $"worktree add {WorktreeDirectory} -b {BranchName}") };
        createWorktree.Start();
        createWorktree.WaitForExit();
        if (createWorktree.ExitCode != 0)
        {
            var createWorktreeError = createWorktree.StandardError.ReadToEnd();
            var failureMessage = $"Creating the test worktree failed with error message:  {createWorktreeError}";
            throw new InvalidOperationException(failureMessage);
        }
    }

    private void PruneOrphanedWorktrees()
    {
        var pruneOrphanedWorktrees = new Process { StartInfo = ConfigureProcessStartInfo("git.exe", "worktree prune") };
        pruneOrphanedWorktrees.Start();
        pruneOrphanedWorktrees.WaitForExit();
        if (pruneOrphanedWorktrees.ExitCode != 0)
        {
            var pruneOrphanedWorktreesError = pruneOrphanedWorktrees.StandardError.ReadToEnd();
            var failureMessage = $"Pruning orphaned working trees failed with error message:  {pruneOrphanedWorktreesError}";
            throw new InvalidOperationException(failureMessage);
        }
    }

    private void DeleteTestWorktreeDirectory()
    {
        if (Directory.Exists(WorktreeDirectory))
        {
            Directory.Delete(WorktreeDirectory, recursive: true);
        }
    }

    private ProcessStartInfo ConfigureProcessStartInfo(string fileName, string arguments)
    {
        return new ProcessStartInfo(fileName, arguments)
               {
                   RedirectStandardError = true,
                   RedirectStandardOutput = true
               };
    }

    private void RemoveTestBranch()
    {
        var removeBranch = new Process { StartInfo = ConfigureProcessStartInfo("git.exe", $"branch -D {BranchName}") };
        removeBranch.Start();
        removeBranch.WaitForExit();
        if (removeBranch.ExitCode != 0)
        {
            var removeBranchError = removeBranch.StandardError.ReadToEnd();
            var failureMessage = $"Removing the test branch ({BranchName}) failed with error message:  {removeBranchError}";
            throw new InvalidOperationException(failureMessage);
        }
    }

    private void RemoveTestWorktree()
    {
        var removeWorktree = new Process { StartInfo = ConfigureProcessStartInfo("git.exe", $"worktree remove {WorktreeDirectory} --force --force") };
        removeWorktree.Start();
        removeWorktree.WaitForExit();
        if (removeWorktree.ExitCode != 0)
        {
            var removeWorktreeError = removeWorktree.StandardError.ReadToEnd();
            var failureMessage = $"Removing the test worktree failed with error message:  {removeWorktreeError}";
            throw new InvalidOperationException(failureMessage);
        }
    }

    private void DisposeInternal()
    {
        try
        {
            RemoveTestWorktree();
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
            // Swallow exceptions.
        }

        try
        {
            RemoveTestBranch();
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
            // Swallow exceptions.
        }

        // Remove the working tree destination if it still exists.  It shouldn't but this will make things run more smoothly.
        try
        {
            DeleteTestWorktreeDirectory();
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
            // Swallow exceptions.
        }
    }
}
