// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2008.07.31

using NUnit.Framework;
using PostSharp.Extensibility;

namespace Xtensive.Core.Aspects.Tests
{
  [TestFixture]
  public class ErrorLogTest
  {
    [Test]
    public static void CombinedTest()
    {
      ErrorLog.Write(SeverityType.Warning, "Warning message: {0}", "message");
      ErrorLog.Debug("Debug message: {0}", "message");
    }
  }
}