// Copyright (C) 2008-2025 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
// Created by: Alex Yakunin
// Created:    2008.02.09

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using NUnit.Framework;

namespace Xtensive.Orm.Tests
{
  /// <summary>
  /// Defines subset of Github Actions events
  /// which may trigger test runs
  /// </summary>
  public enum GithubActionsEvents
  {
    PullRequest,
    WorkflowDispatch,
    WorkflowCall,
    WorkflowRun,
    Push,
    Schedule
  }

  /// <summary>
  /// Provides various info related to the current test.
  /// </summary>
  public static class TestInfo
  {
    private static readonly bool isBuildServer;
    private static readonly bool isGithubActions;
    private static readonly bool noIgnoreOnGithubActions;

    private static readonly GithubActionsEvents? githubActionsTriggeredBy;

    /// <summary>
    /// <see langword="true"/>, if performance test is running (i.e. a test with
    /// "Performance" category).
    /// </summary>
    /// <remarks>
    /// Currently only NUnit tests are recognized.
    /// </remarks>
    public static bool IsPerformanceTestRunning => GetMethodAttributes<CategoryAttribute>().Any(ca => ca.Name == "Performance");

    /// <summary>
    /// <see langword="true"/>, if performance test is running (i.e. a test with
    /// "Performance" category).
    /// </summary>
    /// <remarks>
    /// Currently only NUnit tests are recognized.
    /// </remarks>
    public static bool IsProfileTestRunning => GetMethodAttributes<CategoryAttribute>().Any(ca => ca.Name == "Profile");

    /// <summary>
    /// Gets a value indicating whether test is running under build server.
    /// </summary>
    public static bool IsBuildServer => isBuildServer;

    /// <summary>
    /// Gets a value indicating whether test is running within GitHub Actions environment.
    /// </summary>
    public static bool IsGithubActions => isGithubActions;

    /// <summary>
    /// In case of run on GinHubActions, no test ignore happens in <see cref="IgnoreIfGithubActionsAttribute"/> nor <see cref="IgnoreOnGithubActionsIfFailedAttribute"/>
    /// </summary>
    public static bool NoIgnoreOnGithubActions => noIgnoreOnGithubActions;

    /// <summary>
    /// Gets the event that triggered test run within Github Actions environment.
    /// </summary>
    public static GithubActionsEvents? GithubActionTrigger => githubActionsTriggeredBy;

    private static IEnumerable<T> GetMethodAttributes<T>() where T : Attribute
    {
      var stackFrames = new StackTrace().GetFrames();
      foreach (var frame in stackFrames) {
        var method = frame.GetMethod();
        if (method.GetCustomAttribute<TestAttribute>(false) == null)


          continue;
        foreach (var ca in method.GetCustomAttributes<T>(false))
          yield return ca;

      }
      yield break;
    }

    private static GithubActionsEvents? TryParseGithubEventName(string varValue)
    {
      return varValue switch {
        "pull_request" => GithubActionsEvents.PullRequest,
        "pull_request_comment" => GithubActionsEvents.PullRequest,
        "pull_request_review" => GithubActionsEvents.PullRequest,
        "pull_request_target" => GithubActionsEvents.PullRequest,
        "workflow_dispatch" => GithubActionsEvents.WorkflowDispatch,
        "workflow_call" => GithubActionsEvents.WorkflowCall,
        "workflow_run" => GithubActionsEvents.WorkflowRun,
        "schedule" => GithubActionsEvents.Schedule,
        _ => null
      };
    }

    static TestInfo()
    {
      isBuildServer = Environment.GetEnvironmentVariable("TEAMCITY_VERSION") != null;
      isGithubActions = !string.IsNullOrEmpty(Environment.GetEnvironmentVariable("GITHUB_WORKSPACE"));
      noIgnoreOnGithubActions = isGithubActions && string.Equals(Environment.GetEnvironmentVariable("GA_NO_IGNORE"), "true", StringComparison.OrdinalIgnoreCase);
      githubActionsTriggeredBy = TryParseGithubEventName(Environment.GetEnvironmentVariable("GITHUB_EVENT_NAME"));
    }
  }
}