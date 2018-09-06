// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2009.07.13

using System;
using NUnit.Framework;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.InteropServices;
using Xtensive.Core;
using Xtensive.Orm.Configuration;
using Xtensive.Orm.Providers;
using Xtensive.Orm.Tests.Storage.DbTypeSupportModel;

namespace Xtensive.Orm.Tests.Storage.Providers.Sql
{
  [TestFixture]
  public class StringOperationsTest : AutoBuildTest
  {
    #region Configuration

    private bool emptyStringIsNull;

    protected override DomainConfiguration BuildConfiguration()
    {
      var configuration = base.BuildConfiguration();
      configuration.Types.Register(typeof (X).Assembly, typeof (X).Namespace);
      return configuration;
    }

    public override void TestFixtureSetUp()
    {
      base.TestFixtureSetUp();
      CreateSessionAndTransaction();
      emptyStringIsNull = ProviderInfo.Supports(ProviderFeatures.TreatEmptyStringAsNull);
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
        "ololo[ololo",
        "ololo]ololo",
        // other test values
        "     ",
      };
      foreach (var value in testValues)
        new X {FString = value};
      if (!emptyStringIsNull)
        new X {FString = string.Empty};
    }

    #endregion

    [Test]
    public void LengthTest()
    {
      var results = Session.Demand().Query.All<X>().Select(x => new {
        String = x.FString,
        Length = x.FString.Length
      }).ToList();
      foreach (var item in results)
        Assert.AreEqual(ConvertString(item.String).Length, item.Length);
    }

    #region Trim, TrimStart, TrimEnd

    [Test]
    public void TrimSpaceTest()
    {
      var results = Session.Demand().Query.All<X>()
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
        Assert.AreEqual(ConvertString(x.String).Trim(), ConvertString(x.StringTrim));
        Assert.AreEqual(ConvertString(x.String).TrimStart(), ConvertString(x.StringTrimLeading));
        Assert.AreEqual(ConvertString(x.String).TrimEnd(), ConvertString(x.StringTrimTrailing));
        Assert.AreEqual(ConvertString(x.String).Trim(null), ConvertString(x.StringTrimNull));
        Assert.AreEqual(ConvertString(x.String).TrimStart(null), ConvertString(x.StringTrimLeadingNull));
        Assert.AreEqual(ConvertString(x.String).TrimEnd(null), ConvertString(x.StringTrimTrailingNull));
        Assert.AreEqual(ConvertString(x.String).Trim(' '), ConvertString(x.StringTrimSpace));
        Assert.AreEqual(ConvertString(x.String).TrimStart(' '), ConvertString(x.StringTrimLeadingSpace));
        Assert.AreEqual(ConvertString(x.String).TrimEnd(' '), ConvertString(x.StringTrimTrailingSpace));
      }
    }

    [Test]
    public void TrimOtherCharTest()
    {
      Require.ProviderIsNot(StorageProvider.SqlServer | StorageProvider.SqlServerCe);
      var results = Session.Demand().Query.All<X>()
        .Select(x => new {
          String = x.FString,
          StringTrimLeadingLargePLetter = x.FString.TrimStart('P'),
          StringTrimSmallOLetter = x.FString.Trim('o'),
          StringTrimTrailingClosingBracket = x.FString.TrimEnd(')'),
        }).ToList();
      foreach (var x in results) {
        Assert.AreEqual(ConvertString(x.String).TrimStart('P'), ConvertString(x.StringTrimLeadingLargePLetter));
        Assert.AreEqual(ConvertString(x.String).Trim('o'), ConvertString(x.StringTrimSmallOLetter));
        Assert.AreEqual(ConvertString(x.String).TrimEnd(')'), ConvertString(x.StringTrimTrailingClosingBracket));
      }
    }

    [Test]
    public void TrimMultipleCharsTest()
    {
      Require.ProviderIsNot(StorageProvider.SqlServer | StorageProvider.Oracle | StorageProvider.SqlServerCe);
      var results = Session.Demand().Query.All<X>()
        .Select(x => new {
          String = x.FString,
          StringTrimLeadingZeroAndOne = x.FString.TrimStart('0', '1'),
        }).ToList();
      foreach (var x in results)
        Assert.AreEqual(ConvertString(x.String).TrimStart('0', '1'), ConvertString(x.StringTrimLeadingZeroAndOne));
    }


    #endregion

    #region StartsWith, EndsWith, Contains

    [Test]
    public void StartsWithTest()
    {
      GenericLikeTest(param => x => x.FString.StartsWith(param));
    }

    [Test]
    public void EndsWithTest()
    {
      GenericLikeTest(param => x => x.FString.EndsWith(param));
    }

    [Test]
    public void ContainsTest()
    {
      GenericLikeTest(param => x => x.FString.Contains(param));
    }

    private void GenericLikeTest(Func<string, Expression<Func<X, bool>>> selector)
    {
      var paramStrings = new[] {"A", "%", "_", "%_", "_%", "^", "[", "]"};
      var testStrings = Session.Demand().Query.All<X>().ToArray();
      foreach (var testString in paramStrings) {
        var sample = testStrings.AsQueryable().Where(selector(testString)).Select(x => x.FString).ToArray();
        var result = Session.Demand().Query.All<X>().Where(selector(testString)).Select(x => x.FString).ToArray();
        Assert.IsTrue(sample.SequenceEqual(result), "String: " + testString);
      }
    }

    #endregion

    #region PadLeft, PadRight

    [Test]
    public void PaddingTest()
    {
      var result = Session.Demand().Query.All<X>().Select(x => new {
        x.Id,
        String = x.FString,
        PadLeft = x.FString.PadLeft(10),
        PadRight = x.FString.PadRight(10),
        PadLeftX = x.FString.PadLeft(10, 'X'),
        PadRightX = x.FString.PadRight(10, 'X')
      }).ToList();
      foreach (var item in result) {
        Assert.AreEqual(ConvertString(item.String).PadLeft(10), ConvertString(item.PadLeft));
        Assert.AreEqual(ConvertString(item.String).PadRight(10), ConvertString(item.PadRight));
        Assert.AreEqual(ConvertString(item.String).PadLeft(10, 'X'), ConvertString(item.PadLeftX));
        Assert.AreEqual(ConvertString(item.String).PadRight(10, 'X'), ConvertString(item.PadRightX));
      }
    }

    #endregion

    private string ConvertString(string value)
    {
      return emptyStringIsNull && value==null ? string.Empty : value;
    }
  }
}