// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2007.10.03

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Security;

namespace Xtensive.Orm.Tests
{
  /// <summary>
  /// Provides various debugging related information.
  /// </summary>
  public static class DebugInfo
  {
    private static int isUnitTestSessionRunning;
    private static int isRunningOnBuildServer;
    private static Dictionary<string, bool> testFixtureAttributes;
    private static Dictionary<string, bool> buildServerLaunchers;
    private static Dictionary<string, bool> buildServerEnvironmentVariables;

    /// <summary>
    /// <see langword="True"/>, if current method is executed under unit test runner.
    /// </summary>
    /// <remarks>
    /// Currently only NUnit tests are recognized.
    /// Note that value of this property is detected just once - during its first call.
    /// </remarks>
    public static bool IsUnitTestSessionRunning
    {
      get {
        if (isUnitTestSessionRunning==0) {
          int newIsUnitTestSessionRunning = -1;
          StackFrame[] stackFrames = new StackTrace().GetFrames();
          foreach (StackFrame frame in stackFrames) {
            Type type = frame.GetMethod().DeclaringType;
            Attribute[] typeAttributes = Attribute.GetCustomAttributes(type, false);
            for (int i = 0; i < typeAttributes.Length; i++) {
              bool ignore;
              if (testFixtureAttributes.TryGetValue(typeAttributes[i].GetType().FullName, out ignore)) {
                newIsUnitTestSessionRunning = 1;
                break;
              }
            }
            if (newIsUnitTestSessionRunning==1)
              break;
          }
          isUnitTestSessionRunning = newIsUnitTestSessionRunning;
        }
        return isUnitTestSessionRunning==1;
      }
    }

    /// <summary>
    /// <see langword="True"/>, if current method is executed on build server.
    /// </summary>
    /// <remarks>
    /// Currently only TeamCity is recognized.
    /// Note that value of this property is detected just once - during its first call.
    /// </remarks>
    public static bool IsRunningOnBuildServer
    {
      [SecuritySafeCritical]
      get {
        if (isRunningOnBuildServer==0) {
          int newIsRunningOnBuildServer = -1;
          Process p = Process.GetCurrentProcess();
          string exeName = Path.GetFileName(p.StartInfo.FileName);
          if (buildServerLaunchers.ContainsKey(exeName))
            newIsRunningOnBuildServer = 1;
          else {
            foreach (DictionaryEntry e in p.StartInfo.EnvironmentVariables) {
              if (buildServerEnvironmentVariables.ContainsKey(e.Key as string)) {
                newIsRunningOnBuildServer = 1;
                break;
              }
            }
          }
          isRunningOnBuildServer = newIsRunningOnBuildServer;
        }
        return isRunningOnBuildServer==1;
      }
    }

    // Constructors

    static DebugInfo()
    {
      testFixtureAttributes = new Dictionary<string, bool>();
      testFixtureAttributes.Add("NUnit.Framework.TestFixtureAttribute", true);

      buildServerLaunchers = new Dictionary<string, bool>();
      buildServerLaunchers.Add("JetBrains.BuildServer.NUnitLauncher1.1.exe", true);
      buildServerLaunchers.Add("JetBrains.BuildServer.NUnitLauncher2.0.exe", true);
      buildServerLaunchers.Add("JetBrains.BuildServer.NUnitLauncher2.0.VSTS.exe", true);

      buildServerEnvironmentVariables = new Dictionary<string, bool>();
      buildServerEnvironmentVariables.Add("teamcity.dotnet.nunitlauncher.msbuild.task", true);
      buildServerEnvironmentVariables.Add("teamcity.dotnet.nunitlauncher1.1", true);
      buildServerEnvironmentVariables.Add("teamcity.dotnet.nunitlauncher2.0", true);
    }
  }
}