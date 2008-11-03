// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2008.11.03

using System;
using System.Linq;
using NUnit.Framework;
using Xtensive.Core.Collections;

namespace Xtensive.Storage.Tests
{
  [TestFixture]
  public class TestTest
  {
    [Test]
    public void CombinedTest()
    {
      var array = new[] {"Alex", "Denis", "Misha"};
      Log.Info(array
        .Where(s => s.Length>=5)
        .Select(s => s.Substring(0,2))
        .ToCommaDelimitedString());
      Log.Warning("Test");
    }
  }
}