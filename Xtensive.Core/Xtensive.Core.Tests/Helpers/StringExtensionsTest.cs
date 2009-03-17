// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2009.03.17

using NUnit.Framework;
using Xtensive.Core.Helpers;
using Xtensive.Core.Testing;
using Xtensive.Core.Collections;

namespace Xtensive.Core.Tests.Helpers
{
  [TestFixture]
  public class StringExtensionsTest
  {
    [Test]
    public void RevertibleSplitJoinTest()
    {
      Assert.AreEqual("A,B,C", new[] {"A", "B", "C"}.RevertibleJoin('\\', ','));
      AssertEx.AreEqual(new[] {"A", "B", "C"}, "A,B,C".RevertibleSplit('\\', ','));

      Assert.AreEqual("A,B", new[] {"A", "B"}.RevertibleJoin('\\', ','));
      AssertEx.AreEqual(new[] {"A", "B"}, "A,B".RevertibleSplit('\\', ','));

      Assert.AreEqual("A", new[] {"A"}.RevertibleJoin('\\', ','));
      AssertEx.AreEqual(new[] {"A"}, "A".RevertibleSplit('\\', ','));

      Assert.AreEqual("", new[] {""}.RevertibleJoin('\\', ','));
      Assert.AreEqual("", new string[] {}.RevertibleJoin('\\', ','));
      AssertEx.AreEqual(new[] {""}, "".RevertibleSplit('\\', ','));

      Assert.AreEqual("\\,", new[] {","}.RevertibleJoin('\\', ','));
      AssertEx.AreEqual(new[] {","}, "\\,".RevertibleSplit('\\', ','));

      Assert.AreEqual("\\,,", new[] {",",""}.RevertibleJoin('\\', ','));
      AssertEx.AreEqual(new[] {",", ""}, "\\,,".RevertibleSplit('\\', ','));
    }
  }
}