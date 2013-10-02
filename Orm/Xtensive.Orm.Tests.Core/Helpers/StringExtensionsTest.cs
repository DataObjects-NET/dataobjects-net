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
    public void FormatWithTest()
    {
      // Compatibility with default behavior
      Assert.AreEqual("1", "{0}".FormatWith(1));
      // New behavior with formatting & this
      Assert.AreEqual("1", "{this}".FormatWith(1));
      Assert.AreEqual(" 1", "{this,2}".FormatWith(1));
      Assert.AreEqual(" 1.1", "{this,4:F1}".FormatWith(1.1).Replace(",", "."));
      // New behavior (property access)
      Assert.AreEqual("A,B", "{X},{Y}".FormatWith(new {X = "A", Y = "B"}));
      Assert.AreEqual("1",  "{Seconds}".FormatWith(TimeSpan.FromSeconds(61)));
      Assert.AreEqual("61", "{TotalSeconds}".FormatWith(TimeSpan.FromSeconds(61)));
    }

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