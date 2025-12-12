// Copyright (C) 2009-2020 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
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
      Assert.That(new[] {"A", "B", "C"}.RevertibleJoin('\\', ','), Is.EqualTo("A,B,C"));
      AssertEx.HasSameElements(new[] {"A", "B", "C"}, "A,B,C".RevertibleSplit('\\', ','));

      Assert.That(new[] {"A", "B"}.RevertibleJoin('\\', ','), Is.EqualTo("A,B"));
      AssertEx.HasSameElements(new[] {"A", "B"}, "A,B".RevertibleSplit('\\', ','));

      Assert.That(new[] {"A"}.RevertibleJoin('\\', ','), Is.EqualTo("A"));
      AssertEx.HasSameElements(new[] {"A"}, "A".RevertibleSplit('\\', ','));

      Assert.That(new[] {""}.RevertibleJoin('\\', ','), Is.EqualTo(""));
      Assert.That(new string[] {}.RevertibleJoin('\\', ','), Is.EqualTo(""));
      AssertEx.HasSameElements(new[] {""}, "".RevertibleSplit('\\', ','));

      Assert.That(new[] {","}.RevertibleJoin('\\', ','), Is.EqualTo("\\,"));
      AssertEx.HasSameElements(new[] {","}, "\\,".RevertibleSplit('\\', ','));

      Assert.That(new[] {",",""}.RevertibleJoin('\\', ','), Is.EqualTo("\\,,"));
      AssertEx.HasSameElements(new[] {",", ""}, "\\,,".RevertibleSplit('\\', ','));
    }

    [Test]
    public void IndentTest()
    {
      Assert.That("A".Indent(0), Is.EqualTo("A"));
      Assert.That("A".Indent(1), Is.EqualTo(" A"));
      Assert.That("A".Indent(2), Is.EqualTo("  A"));

      Assert.That("A".Indent(1, false), Is.EqualTo("A"));
      Assert.That("A\r\nB".Indent(1, false), Is.EqualTo("A\r\n B"));
      Assert.That("A\r\nB".Indent(1, true), Is.EqualTo(" A\r\n B"));
    }

    [Test]
    public void LikeExtensionTest()
    {
      _ = Assert.Throws<ArgumentException>(() => {
        Assert.That("uewryewsf".Like("%sf"), Is.EqualTo(true));
        Assert.That("s__asdf".Like("_%asdf"), Is.EqualTo(true));
        Assert.That("dsfEEEE".Like("dsf%"), Is.EqualTo(true));
        Assert.That("Afigdf".Like("_figdf"), Is.EqualTo(true));
        Assert.That("fsdfASDsdfs".Like("fsdf___sdfs"), Is.EqualTo(true));
        Assert.That("my name is Alex.".Like("my name is _____"), Is.EqualTo(true));
        Assert.That("how old are you?".Like("how old % you_"), Is.EqualTo(true));
        Assert.That("hi, I'm alex. I'm 26".Like("hi, I'm ____. I'm %"), Is.EqualTo(true));
        Assert.That("it's another test string%%%".Like("it's another test string!%!%!%"), Is.EqualTo(false));
        Assert.That("it's another test string%%%".Like("it's another test string!%!%!%", '!'), Is.EqualTo(true));
        Assert.That("string with error.".Like("String with error_"), Is.EqualTo(false));
        Assert.That("Another string with error.".Like("another string with err%."), Is.EqualTo(false));
        Assert.That("aRRRRa%".Like("a%a%%", '%'), Is.EqualTo(true));
      });
    }

    [Test]
    public void TrimRoundBracketsSymetricallyTest()
    {
      Assert.That("(abc)".StripRoundBrackets(), Is.EqualTo("abc"));
      Assert.That("abc".StripRoundBrackets(), Is.EqualTo("abc"));
      Assert.That("((((Convert(abc)))))".StripRoundBrackets(), Is.EqualTo("Convert(abc)"));
      Assert.That("(Convert(abc))".StripRoundBrackets(), Is.EqualTo("Convert(abc)"));
      Assert.That("((((abc))".StripRoundBrackets(), Is.EqualTo("((abc"));
      Assert.That("((abc))))".StripRoundBrackets(), Is.EqualTo("abc))"));
    }
  }
}