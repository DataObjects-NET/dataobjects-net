// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2007.10.11

using NUnit.Framework;
using Xtensive.Diagnostics;

namespace Xtensive.Orm.Tests.Core.Diagnostics
{
  [TestFixture]
  public class LogIndentTest
  {
    [Test]
    public void CombinedTest()
    {
      TestLog.Info("Not indented");
      using (new LogIndentScope()) {
        using (new LogCaptureScope(LogProvider.ConsoleLog)) {
          TestLog.Info("Indented (1)");
          using (new LogIndentScope()) {
            TestLog.Info("Indented (2)");
          }
        }
      }
    }
  }
}