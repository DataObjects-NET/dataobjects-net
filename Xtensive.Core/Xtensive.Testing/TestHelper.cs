// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2008.02.09

using System;
using System.IO;
using System.Threading;
using NUnit.Framework;
using Xtensive.Core;
using Xtensive.Helpers;
using Xtensive.Testing.Resources;

namespace Xtensive.Testing
{
  /// <summary>
  /// Test helper class.
  /// </summary>
  public static class TestHelper
  {
    private const int MaxTestFolderCount = 99;
    private static volatile string testFolderName = null;
    private static object _lock = new object();

    /// <summary>
    /// Gets temporary folder name (for tests only).
    /// </summary>
    /// <remarks>
    /// The folder name is generated automatically.
    /// Such folders are removed either in <see cref="AppDomain.DomainUnload"/> (if possible),
    /// or on the next attempt to read this property after the next application startup.
    /// </remarks>
    public static string TestFolderName {
      get {
        if (testFolderName==null) lock (_lock) if (testFolderName==null) {
          string tempFolder = Environment.GetEnvironmentVariable("TEMP");
          if (tempFolder.IsNullOrEmpty())
            tempFolder = "C:\\Temp";
          bool done = false;
          for (int i = 0; i<MaxTestFolderCount; i++) {
            string folderName = Path.Combine(tempFolder, string.Format(Strings.TestFolderNameFormat, i));
            if (Directory.Exists(folderName)) 
              try {
                Directory.Delete(folderName, true);
              }
              catch {
                continue;
              }
            if (!done) {
              try {
                Directory.CreateDirectory(folderName);
              }
              catch {
                continue;
              }
              testFolderName = folderName;
              AppDomain.CurrentDomain.DomainUnload += delegate(object sender, EventArgs e) {
                if (testFolderName==null) lock (_lock) if (testFolderName==null) {
                  if (Directory.Exists(folderName))
                    try {
                      Directory.Delete(folderName, true);
                    }
                    catch {
                    }
                }
              };
              done = true;
            }
          }
          if (!done)
            throw new InvalidOperationException(string.Format(Strings.ExCantCreateTestFolder, 
              string.Format(Strings.TestFolderNameFormat, 0),
              string.Format(Strings.TestFolderNameFormat, MaxTestFolderCount)));
        }
        return testFolderName;
      }
    }

    /// <summary>
    /// Ensures full garbage collection.
    /// </summary>
    public static void CollectGarbage()
    {
      CollectGarbage(TestInfo.IsPerformanceTestRunning);
    }

    /// <summary>
    /// Ensures full garbage collection.
    /// </summary>
    /// <param name="preferFullRatherThanFast">Full rather then fast collection should be performed.</param>
    public static void CollectGarbage(bool preferFullRatherThanFast)
    {
      int baseSleepTime = 1;
      if (preferFullRatherThanFast)
        baseSleepTime = 100;

      for (int i = 0; i<5; i++) {
        Thread.Sleep(baseSleepTime);
        GC.GetTotalMemory(true);
        GC.WaitForPendingFinalizers();
      }
    }
  }
}