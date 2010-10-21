// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2008.02.09

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using NUnit.Framework;

namespace Xtensive.Testing
{
  /// <summary>
  /// Provides various info related to the current test.
  /// </summary>
  public static class TestInfo
  {
    /// <summary>
    /// <see langword="True"/>, if performance test is running (i.e. a test with
    /// "Performance" category).
    /// </summary>
    /// <remarks>
    /// Currently only NUnit tests are recognized.
    /// </remarks>
    public static bool IsPerformanceTestRunning
    {
      get
      {
        foreach (CategoryAttribute attribute in GetMethodAttributes<CategoryAttribute>())
          if (attribute.Name=="Performance")
            return true;

        return false;
      }
    }

    /// <summary>
    /// <see langword="True"/>, if performance test is running (i.e. a test with
    /// "Performance" category).
    /// </summary>
    /// <remarks>
    /// Currently only NUnit tests are recognized.
    /// </remarks>
    public static bool IsProfileTestRunning
    {
      get
      {
        foreach (CategoryAttribute attribute in GetMethodAttributes<CategoryAttribute>())
          if (attribute.Name=="Profile")
            return true;

        return false;
      }
    }

    /// <summary>
    /// Gets a value indicating whether test is running under build server.
    /// </summary>
    public static bool IsBuildServer {
      get {
        return Environment.GetEnvironmentVariable("TEAMCITY_VERSION")!=null;
      }
    }

    private static IEnumerable<T> GetMethodAttributes<T>() where T : Attribute
    {
      StackFrame[] stackFrames = new StackTrace().GetFrames();
      foreach (StackFrame frame in stackFrames) {
        MethodBase method = frame.GetMethod();
        // ѕочему сразу не вз€ть нужные атрибуты, например, CategoryAttribute?
        Attribute[] methodAttributes = Attribute.GetCustomAttributes(method, typeof (TestAttribute), false);
        if (methodAttributes==null || methodAttributes.Length==0)
          continue;
        methodAttributes = Attribute.GetCustomAttributes(method, typeof (T), false);
        for (int i = 0; i < methodAttributes.Length; i++)
          yield return (T) methodAttributes[i];
      }
      yield break;
    }
  }
}