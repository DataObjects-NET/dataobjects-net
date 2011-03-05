// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2009.06.15

using NUnit.Framework;
using Xtensive.Core;
using Xtensive.Testing;

namespace Xtensive.Tests
{
  [TestFixture]
  public class ArgumentValidatorTest
  {
    [Test]
    public void CombinedTest()
    {
      ArgumentValidator.EnsureArgumentIsGreaterThan(1, 0, "x");
      AssertEx.ThrowsArgumentOutOfRangeException(() => ArgumentValidator.EnsureArgumentIsGreaterThan(0, 0, "x"));
      AssertEx.ThrowsArgumentOutOfRangeException(() => ArgumentValidator.EnsureArgumentIsGreaterThan(-1, 0, "x"));

      ArgumentValidator.EnsureArgumentIsLessThan(-1, 0, "x");
      AssertEx.ThrowsArgumentOutOfRangeException(() => ArgumentValidator.EnsureArgumentIsLessThan(0, 0, "x"));
      AssertEx.ThrowsArgumentOutOfRangeException(() => ArgumentValidator.EnsureArgumentIsLessThan(1, 0, "x"));
    }
  }
}