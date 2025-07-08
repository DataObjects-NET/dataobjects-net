// Copyright (C) 20082025 Xtensive LLC.
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
  /// Provides various info related to the current test.
  /// </summary>
  public static class TestInfo
  {
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
    public static bool IsBuildServer => Environment.GetEnvironmentVariable("TEAMCITY_VERSION") != null;

    /// <summary>
    /// Gets a value indicating whether test is running within GitHub Actions environment.
    /// </summary>
    public static bool IsGitHubActions => Environment.GetEnvironmentVariable("GITHUB_WORKSPACE") != null;

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
  }
}