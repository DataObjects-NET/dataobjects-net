// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2009.07.13

using System.Linq;
using NUnit.Framework;
using Xtensive.Core.Disposing;
using Xtensive.Core.Testing;
using Xtensive.Storage.Configuration;
using Xtensive.Storage.Tests.Storage.DbTypeSupportModel;

namespace Xtensive.Storage.Tests.Storage.Providers.Sql
{
  [TestFixture]
  public class StringOperationsTest : AutoBuildTest
  {
    #region Configuration

    private DisposableSet disposableSet;

    protected override void CheckRequirements()
    {
      EnsureProtocolIs(StorageProtocol.Sql);
    }

    protected override DomainConfiguration BuildConfiguration()
    {
      var configuration = base.BuildConfiguration();
      configuration.Types.Register(typeof (X).Assembly, typeof (X).Namespace);
      return configuration;
    }

    public override void TestFixtureSetUp()
    {
      base.TestFixtureSetUp();
      disposableSet = new DisposableSet();
      disposableSet.Add(Domain.OpenSession());
      disposableSet.Add(Transaction.Open());

      var testValues = new[] {
        // test values for TrimStart, TrimEnd, Trim
        " :-P  ",
        ";-)",
        " :-)",
        ":-( ",
        "0_o",
        "o_0",
        "0_0",
        "o_o",
        "1111oneoneoneo",
        ")))))",
        "?????? ",
        " PROFIT",
        // test values for StartsWith, EndsWith, Contains
        "AAA",
        "%_%",
        "AB_",
        "POW",
        "__",
        "^QQ",
        "PQ^",
        // other test values
        "",
        "     ",
      };
      foreach (var value in testValues)
        new X {FString = value};
    }

    public override void TestFixtureTearDown()
    {
      disposableSet.DisposeSafely();
      base.TestFixtureTearDown();
    }

    #endregion

    [Test]
    public void LengthTest()
    {
      var results = Query<X>.All.Select(x => new {
        String = x.FString,
        Length = x.FString.Length
      }).ToList();
      foreach (var item in results)
        Assert.AreEqual(item.String.Length, item.Length);
    }

    #region Trim, TrimStart, TrimEnd

    [Test]
    public void TrimSpaceTest()
    {
      var results = Query<X>.All
        .Select(x => new {
          String = x.FString,
          StringTrim = x.FString.Trim(),
          StringTrimLeading = x.FString.TrimStart(),
          StringTrimTrailing = x.FString.TrimEnd(),
          StringTrimNull = x.FString.Trim(null),
          StringTrimLeadingNull = x.FString.TrimStart(null),
          StringTrimTrailingNull = x.FString.TrimEnd(null),
          StringTrimSpace = x.FString.Trim(' '),
          StringTrimLeadingSpace = x.FString.TrimStart(' '),
          StringTrimTrailingSpace = x.FString.TrimEnd(' '),
        }).ToList();
      foreach (var x in results) {
        Assert.AreEqual(x.String.Trim(), x.StringTrim);
        Assert.AreEqual(x.String.TrimStart(), x.StringTrimLeading);
        Assert.AreEqual(x.String.TrimEnd(), x.StringTrimTrailing);
        Assert.AreEqual(x.String.Trim(null), x.StringTrimNull);
        Assert.AreEqual(x.String.TrimStart(null), x.StringTrimLeadingNull);
        Assert.AreEqual(x.String.TrimEnd(null), x.StringTrimTrailingNull);
        Assert.AreEqual(x.String.Trim(' '), x.StringTrimSpace);
        Assert.AreEqual(x.String.TrimStart(' '), x.StringTrimLeadingSpace);
        Assert.AreEqual(x.String.TrimEnd(' '), x.StringTrimTrailingSpace);
      }
    }

    [Test]
    public void TrimOtherCharsTest()
    {
      EnsureProtocolIs(~StorageProtocol.SqlServer);
      var results = Query<X>.All
        .Select(x => new {
          String = x.FString,
          StringTrimLeadingZeroAndOne = x.FString.TrimStart('0', '1'),
          StringTrimTrailingClosingBracket = x.FString.TrimEnd(')'),
          StringTrimSmallOLetter = x.FString.Trim('o'),
        }).ToList();
      foreach (var x in results) {
        Assert.AreEqual(x.String.TrimStart('0', '1'), x.StringTrimLeadingZeroAndOne);
        Assert.AreEqual(x.String.TrimEnd(')'), x.StringTrimTrailingClosingBracket);
        Assert.AreEqual(x.String.Trim('o'), x.StringTrimSmallOLetter);
      }
    }

    #endregion

    #region StartsWith, EndsWith, Contains

    [Test]
    public void StartsWithTest()
    {
      var result = Query<X>.All.Select(x => new {
        x.Id,
        String = x.FString,
        StartsWithA = x.FString.StartsWith("A"),
        StartsWithPercent = x.FString.StartsWith("%"),
        StartsWithGround = x.FString.StartsWith("_"),
        StartsWithPrecentGround = x.FString.StartsWith("%_"),
        StartsWithGroundPercent = x.FString.StartsWith("_%"),
        StartsWithEscape = x.FString.StartsWith("^"),
      }).ToList();
      foreach (var x in result) {
        Assert.AreEqual(x.String.StartsWith("A"), x.StartsWithA);
        Assert.AreEqual(x.String.StartsWith("%"), x.StartsWithPercent);
        Assert.AreEqual(x.String.StartsWith("_"), x.StartsWithGround);
        Assert.AreEqual(x.String.StartsWith("%_"), x.StartsWithPrecentGround);
        Assert.AreEqual(x.String.StartsWith("_%"), x.StartsWithGroundPercent);
        Assert.AreEqual(x.String.StartsWith("^"), x.StartsWithEscape);
      }
    }

    [Test]
    public void EndsWithTest()
    {
      var result = Query<X>.All.Select(x => new {
        x.Id,
        String = x.FString,
        EndsWithA = x.FString.EndsWith("A"),
        EndsWithPercent = x.FString.EndsWith("%"),
        EndsWithGround = x.FString.EndsWith("_"),
        EndsWithPrecentGround = x.FString.EndsWith("%_"),
        EndsWithGroundPercent = x.FString.EndsWith("_%"),
        EndsWithEscape = x.FString.EndsWith("^"),
      }).ToList();
      foreach (var x in result) {
        Assert.AreEqual(x.String.EndsWith("A"), x.EndsWithA);
        Assert.AreEqual(x.String.EndsWith("%"), x.EndsWithPercent);
        Assert.AreEqual(x.String.EndsWith("_"), x.EndsWithGround);
        Assert.AreEqual(x.String.EndsWith("%_"), x.EndsWithPrecentGround);
        Assert.AreEqual(x.String.EndsWith("_%"), x.EndsWithGroundPercent);
        Assert.AreEqual(x.String.EndsWith("^"), x.EndsWithEscape);
      }
    }

    [Test]
    public void ContainsTest()
    {
      var result = Query<X>.All.Select(x => new {
        x.Id,
        String = x.FString,
        ContainsA = x.FString.Contains("A"),
        ContainsPercent = x.FString.Contains("%"),
        ContainsGround = x.FString.Contains("_"),
        ContainsPrecentGround = x.FString.Contains("%_"),
        ContainsGroundPercent = x.FString.Contains("_%"),
        ContainsEscape = x.FString.Contains("^"),
      }).ToList();
      foreach (var x in result) {
        Assert.AreEqual(x.String.Contains("A"), x.ContainsA);
        Assert.AreEqual(x.String.Contains("%"), x.ContainsPercent);
        Assert.AreEqual(x.String.Contains("_"), x.ContainsGround);
        Assert.AreEqual(x.String.Contains("%_"), x.ContainsPrecentGround);
        Assert.AreEqual(x.String.Contains("_%"), x.ContainsGroundPercent);
        Assert.AreEqual(x.String.Contains("^"), x.ContainsEscape);
      }
    }

    [Test]
    public void NotSupportedTest()
    {
      var localVar = ":-)";
      AssertEx.ThrowsNotSupportedException(
        () => Query<X>.All.Select(x => x.FString.StartsWith(localVar)).ToList());
      AssertEx.ThrowsNotSupportedException(
        () => Query<X>.All.Select(x => x.FString.EndsWith(localVar)).ToList());
      AssertEx.ThrowsNotSupportedException(
        () => Query<X>.All.Select(x => x.FString.Contains(localVar)).ToList());
    }

    #endregion

    #region PadLeft, PadRight

    [Test]
    public void PaddingTest()
    {
      var result = Query<X>.All.Select(x => new {
        x.Id,
        String = x.FString,
        PadLeft = x.FString.PadLeft(10),
        PadRight = x.FString.PadRight(10),
        PadLeftX = x.FString.PadLeft(10, 'X'),
        PadRightX = x.FString.PadRight(10, 'X')
      }).ToList();
      foreach (var item in result) {
        Assert.AreEqual(item.String.PadLeft(10), item.PadLeft);
        Assert.AreEqual(item.String.PadRight(10), item.PadRight);
        Assert.AreEqual(item.String.PadLeft(10, 'X'), item.PadLeftX);
        Assert.AreEqual(item.String.PadRight(10, 'X'), item.PadRightX);
      }
    }

    #endregion
  }
}