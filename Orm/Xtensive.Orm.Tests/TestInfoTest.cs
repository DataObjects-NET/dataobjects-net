using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;

namespace Xtensive.Orm.Tests
{

  [TestFixture]
  public sealed class TestInfoTest
  {
    [Test]
    public void IsBuildServerTest()
    {
      var teamCityVersion = Environment.GetEnvironmentVariable("TEAMCITY_VERSION");
      Console.WriteLine($"TEAMCITY_VERSION : {teamCityVersion}; IsBuildServer : {TestInfo.IsBuildServer}");
      Assert.That(TestInfo.IsBuildServer, Is.EqualTo(teamCityVersion != null));
    }

    [Test]
    public void IsGithubActionsTest()
    {
      var githubActions = Environment.GetEnvironmentVariable("GITHUB_WORKSPACE");
      Console.WriteLine($"GITHUB_WORKSPACE : {githubActions}; IsGithubActions : {TestInfo.IsGithubActions}");

      if (githubActions is null) {
        Assert.That(TestInfo.IsGithubActions, Is.False);

        var githubActionsNoIgnore = Environment.GetEnvironmentVariable("GA_NO_IGNORE");
        Console.WriteLine($"GA_NO_IGNORE : {githubActionsNoIgnore} ; NoIgnoreOnGithubActions : {TestInfo.NoIgnoreOnGithubActions}");
        if (githubActionsNoIgnore is null)
          Assert.That(TestInfo.NoIgnoreOnGithubActions, Is.False);
        else
          Assert.That(TestInfo.NoIgnoreOnGithubActions, Is.EqualTo(githubActionsNoIgnore.Equals("true", StringComparison.OrdinalIgnoreCase)));
      }
      else {
        Assert.That(TestInfo.IsGithubActions, Is.True);

        var githubActionsTrigger = Environment.GetEnvironmentVariable("GITHUB_EVENT_NAME");
        Console.WriteLine($"GITHUB_EVENT_NAME : {githubActionsTrigger} ; GithubActionTrigger : {TestInfo.GithubActionTrigger}");
        if (githubActionsTrigger is null) {
          Assert.That(TestInfo.GithubActionTrigger.HasValue, Is.False);
        }
        else {
          Assert.That(TestInfo.GithubActionTrigger.HasValue, Is.True);
        }

        var githubActionsNoIgnore = Environment.GetEnvironmentVariable("GA_NO_IGNORE");
        Console.WriteLine($"GA_NO_IGNORE : {githubActionsNoIgnore} ; NoIgnoreOnGithubActions : {TestInfo.NoIgnoreOnGithubActions}");
        if (githubActionsNoIgnore is null) {
          Assert.That(TestInfo.NoIgnoreOnGithubActions, Is.False);
        }
        else {
          Assert.That(TestInfo.NoIgnoreOnGithubActions, Is.EqualTo(githubActionsNoIgnore.Equals("true", StringComparison.OrdinalIgnoreCase)));
        }
      }
    }
  }
}
