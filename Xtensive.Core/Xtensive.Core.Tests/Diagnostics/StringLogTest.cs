// Copyright (C) 2007 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: 
// Created:    2007.10.20

using System;
using NUnit.Framework;
using Xtensive.Core.Diagnostics;
using Xtensive.Core.Testing;

namespace Xtensive.Core.Tests.Diagnostics
{
  [TestFixture]
  public class StringLogTest
  {
    private const string Marker = "Marker";

    [Test]
    public void CombinedTest()
    {
      ILog log = StringLog.Create();
      using (new LogCaptureScope(log)) {
        Log.Info("Logging {0}-1", Marker);
        Log.Info("Logging {0}-2", Marker);
      }
      string loggedText = log.Text;
      Console.WriteLine(log.Text);
      AssertEx.IsPatternMatch(loggedText, "*"+Marker+"-1*"+Marker+"-2*");
    }
  }
}