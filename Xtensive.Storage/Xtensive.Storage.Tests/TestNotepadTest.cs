// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2008.11.03

using System;
using System.Linq.Expressions;
using NUnit.Framework;

namespace Xtensive.Storage.Tests
{
  [TestFixture]
  public class TestNotepadTest
  {
    [Test]
    public void LoggingTest()
    {
      Log.Info("Test passed.");
    }
  }
}