﻿using System.Net.Http.Headers;
using System.Threading.Tasks;
using Xunit;

namespace Octokit.Tests.Integration.Clients
{
    public class StatisticsClientTests
    {
        readonly IGitHubClient _client;
        readonly ICommitsClient _fixture;

        public StatisticsClientTests()
        {
            _client = new GitHubClient(new ProductHeaderValue("OctokitTests"))
            {
                Credentials = Helper.Credentials
            };
            _fixture = _client.GitDatabase.Commit;
        }

        [IntegrationTest]
        public async Task CanCreateAndRetrieveCommit()
        {
            var repository = await CreateRepository();
            await CommitToRepository(repository);
            var contributors = await _client.Statistics.GetContributors(repository.Owner, repository.Name);
            Assert.NotNull(contributors);
        }

        [IntegrationTest]
        public async Task CanGetCommitActivityForTheLastYear()
        {
            var repository = await CreateRepository();
            await CommitToRepository(repository);
            var contributors = await _client.Statistics.GetCommitActivityForTheLastYear(repository.Owner, repository.Name);
            Assert.NotNull(contributors);
        }

        async Task<RepositorySummary> CreateRepository()
        {
            var repoName = Helper.MakeNameWithTimestamp("public-repo");
            var repository = await _client.Repository.Create(new NewRepository { Name = repoName, AutoInit = true });
            return new RepositorySummary
            {
                Owner = repository.Owner.Login,
                Name = repoName
            };
        }

        async Task<Commit> CommitToRepository(RepositorySummary repositorySummary)
        {
            var owner = repositorySummary.Owner;
            var repository = repositorySummary.Name;
            var blob = new NewBlob
            {
                Content = "Hello World!",
                Encoding = EncodingType.Utf8
            };
            var blobResult = await _client.GitDatabase.Blob.Create(owner, repository, blob);

            var newTree = new NewTree();
            newTree.Tree.Add(new NewTreeItem
            {
                Type = TreeType.Blob,
                Mode = FileMode.File,
                Path = "README.md",
                Sha = blobResult.Sha
            });

            var treeResult = await _client.GitDatabase.Tree.Create(owner, repository, newTree);

            var newCommit = new NewCommit("test-commit", treeResult.Sha);

            var commit = await _fixture.Create(owner, repository, newCommit);
            return commit;
        }

        class RepositorySummary
        {
            public string Name { get; set; }

            public string Owner { get; set; }
        }
    }
}
