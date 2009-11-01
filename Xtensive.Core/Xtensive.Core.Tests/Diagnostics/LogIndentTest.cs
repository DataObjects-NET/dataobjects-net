// Copyright (C) 2007 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2007.10.11

using NUnit.Framework;
using Xtensive.Core.Diagnostics;

namespace Xtensive.Core.Tests.Diagnostics
{
  [TestFixture]
  public class LogIndentTest
  {
    [Test]
    public void CombinedTest()
    {
      Log.Info("Not indented");
      using (new LogIndentScope()) {
        using (new LogCaptureScope(LogProvider.ConsoleLog)) {
          Log.Info("Indented (1)");
          using (new LogIndentScope()) {
            Log.Info("Indented (2)");
          }
        }
      }
    }
  }
}