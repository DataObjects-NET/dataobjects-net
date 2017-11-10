// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2009.03.17

using System;
using NUnit.Framework;
using Xtensive.Core;
using Xtensive.Orm.Tests;

namespace Xtensive.Orm.Tests.Core.Helpers
{
  [TestFixture]
  public class StringExtensionsTest
  {
    [Test]
    public void RevertibleSplitJoinTest()
    {
      Assert.AreEqual("A,B,C", new[] {"A", "B", "C"}.RevertibleJoin('\\', ','));
      AssertEx.HasSameElements(new[] {"A", "B", "C"}, "A,B,C".RevertibleSplit('\\', ','));

      Assert.AreEqual("A,B", new[] {"A", "B"}.RevertibleJoin('\\', ','));
      AssertEx.HasSameElements(new[] {"A", "B"}, "A,B".RevertibleSplit('\\', ','));

      Assert.AreEqual("A", new[] {"A"}.RevertibleJoin('\\', ','));
      AssertEx.HasSameElements(new[] {"A"}, "A".RevertibleSplit('\\', ','));

      Assert.AreEqual("", new[] {""}.RevertibleJoin('\\', ','));
      Assert.AreEqual("", new string[] {}.RevertibleJoin('\\', ','));
      AssertEx.HasSameElements(new[] {""}, "".RevertibleSplit('\\', ','));

      Assert.AreEqual("\\,", new[] {","}.RevertibleJoin('\\', ','));
      AssertEx.HasSameElements(new[] {","}, "\\,".RevertibleSplit('\\', ','));

      Assert.AreEqual("\\,,", new[] {",",""}.RevertibleJoin('\\', ','));
      AssertEx.HasSameElements(new[] {",", ""}, "\\,,".RevertibleSplit('\\', ','));
    }

    [Test]
    public void IndentTest()
    {
      Assert.AreEqual("A".Indent(0), "A");
      Assert.AreEqual("A".Indent(1), " A");
      Assert.AreEqual("A".Indent(2), "  A");

      Assert.AreEqual("A".Indent(1, false), "A");
      Assert.AreEqual("A\r\nB".Indent(1, false), "A\r\n B");
      Assert.AreEqual("A\r\nB".Indent(1, true), " A\r\n B");
    }

    [Test]
    [ExpectedException(typeof(ArgumentException))]
    public void LikeExtensionTest()
    {
      Assert.AreEqual("uewryewsf".Like("%sf"), true);
      Assert.AreEqual("s__asdf".Like("_%asdf"), true);
      Assert.AreEqual("dsfEEEE".Like("dsf%"), true);
      Assert.AreEqual("Afigdf".Like("_figdf"), true);
      Assert.AreEqual("fsdfASDsdfs".Like("fsdf___sdfs"), true);
      Assert.AreEqual("my name is Alex.".Like("my name is _____"), true);
      Assert.AreEqual("how old are you?".Like("how old % you_"), true);
      Assert.AreEqual("hi, I'm alex. I'm 26".Like("hi, I'm ____. I'm %"), true);
      Assert.AreEqual("it's another test string%%%".Like("it's another test string!%!%!%"), false);
      Assert.AreEqual("it's another test string%%%".Like("it's another test string!%!%!%", '!'), true);
      Assert.AreEqual("string with error.".Like("String with error_"), false);
      Assert.AreEqual("Another string with error.".Like("another string with err%."), false);
      Assert.AreEqual("aRRRRa%".Like("a%a%%", '%'), true);
    }
  }
}