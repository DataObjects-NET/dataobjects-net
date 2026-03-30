// Copyright (C) 2009-2025 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
// Created by: Denis Krjuchkov
// Created:    2009.07.13

using NUnit.Framework;
using System;
using System.Linq;
using Xtensive.Orm.Configuration;
using Xtensive.Orm.Providers;
using Xtensive.Orm.Tests.Storage.DbTypeSupportModel;

namespace Xtensive.Orm.Tests.Storage.Providers.Sql
{
  [TestFixture]
  public class StringOperationsTest : AutoBuildTest
  {
    private const string StringOfWhiteSpaces = "     ";

    #region Configuration

    private readonly string[] testValues = new[] {
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
        "[ololoololo",
        "ololo[ololo",
        "ololoololo[",
        "]ololoololo",
        "ololo]ololo",
        "ololoololo]",
        // other test values
        StringOfWhiteSpaces,
      };


    private bool emptyStringIsNull;
    private bool autoTrimWhiteSpaces;
    private bool whitespaceStringAsEmptyString;

    protected override bool InitGlobalSession => true;

    protected override DomainConfiguration BuildConfiguration()
    {
      var configuration = base.BuildConfiguration();
      configuration.Types.Register(typeof(X).Assembly, typeof(X).Namespace);
      return configuration;
    }

    public override void TestFixtureSetUp()
    {
      base.TestFixtureSetUp();
      InitStringRules();

      var fStrings = emptyStringIsNull ? testValues : testValues.Append(string.Empty);
      foreach (var value in fStrings) {
        _ = new X { FString = value };
      }

      GlobalSession.SaveChanges();
    }

    private void InitStringRules()
    {
      (emptyStringIsNull, whitespaceStringAsEmptyString, autoTrimWhiteSpaces) =
        StorageProviderInfo.Instance.Provider switch {
          StorageProvider.Firebird  => (ProviderInfo.Supports(ProviderFeatures.TreatEmptyStringAsNull), true, true),
          StorageProvider.MySql     => (ProviderInfo.Supports(ProviderFeatures.TreatEmptyStringAsNull), true, false),
          StorageProvider.SqlServer => (ProviderInfo.Supports(ProviderFeatures.TreatEmptyStringAsNull), true, false),
          _                         => (ProviderInfo.Supports(ProviderFeatures.TreatEmptyStringAsNull), false, false)
      };
    }

    #endregion

    [Test]
    public void LengthTest()
    {
      var results = GlobalSession.Query.All<X>().Select(x => new {
        String = x.FString,
        Length = x.FString.Length
      }).ToList();
      foreach (var item in results) {
        Assert.AreEqual(ConvertString(item.String).Length, item.Length);
      }
    }

    [Test]
    public void LengthServerSideTest()
    {
      foreach (var value in testValues) {
        Assert.That(GlobalSession.Query.All<X>().Where(x => x.FString == value && x.FString.Length == value.Length).Count(),
          Is.EqualTo(1), $"Failed for '{value}'.Length");
      }
    }

    [Test]
    public void CharsTest()
    {
      var results = GlobalSession.Query.All<X>()
        .Where(x => x.Id > 5 && x.Id < 11)
        .Select(x => new {
          String = x.FString,
          Char1 = x.FString[1],
          Char2 = x.FString[2]
        })
        .ToList();
      foreach (var item in results) {
        Assert.AreEqual(ConvertString(item.String)[1], item.Char1);
        Assert.AreEqual(ConvertString(item.String)[2], item.Char2);
      }
    }

    [Test]
    public void CharsServerSideTest()
    {
      foreach (var value in testValues.Where(t => t[0] != ' ' && t.Length >= 2)) {
        Assert.That(GlobalSession.Query.All<X>().Where(x => x.FString == value && x.FString[0] == value[0]).Count(),
          Is.EqualTo(1), $"Failed for '{value}'[0]");

        Assert.That(GlobalSession.Query.All<X>().Where(x => x.FString == value && x.FString[1] == value[1]).Count(),
          Is.EqualTo(1), $"Failed for '{value}'[1]");
      }
    }

    #region Trim, TrimStart, TrimEnd

    [Test]
    public void TrimSpaceTest()
    {
      var results = GlobalSession.Query.All<X>()
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
    public void TrimSpaceServerSideTest()
    {
      var checkForWhitespaceString = StorageProviderInfo.Instance.Provider.HasFlag(StorageProvider.Oracle)
        ? (emptyStringIsNull && whitespaceStringAsEmptyString) || autoTrimWhiteSpaces
        : emptyStringIsNull || autoTrimWhiteSpaces || whitespaceStringAsEmptyString;

      foreach (var value in testValues) {
        var expectedValue = value == StringOfWhiteSpaces && checkForWhitespaceString
          ? 2 : 1;

        var result = GlobalSession.Query.All<X>()
          .Where(x => x.FString == value && x.FString.Trim() == value.Trim()).Count();
        Assert.That(result, Is.EqualTo(expectedValue), $"Failed for '{value}'.Trim()");

        result = GlobalSession.Query.All<X>()
          .Where(x => x.FString == value && x.FString.TrimStart() == value.TrimStart()).Count();
        Assert.That(result, Is.EqualTo(expectedValue), $"Failed for '{value}'.TrimStart()");

        result = GlobalSession.Query.All<X>()
          .Where(x => x.FString == value && x.FString.TrimEnd() == value.TrimEnd()).Count();
        Assert.That(result, Is.EqualTo(expectedValue), $"Failed for '{value}'.TrimEnd()");

        result = GlobalSession.Query.All<X>()
          .Where(x => x.FString == value && x.FString.Trim(null) == value.Trim(null)).Count();
        Assert.That(result, Is.EqualTo(expectedValue), $"Failed for '{value}'.Trim(null)");

        result = GlobalSession.Query.All<X>()
          .Where(x => x.FString == value && x.FString.TrimStart(null) == value.TrimStart(null)).Count();
        Assert.That(result, Is.EqualTo(expectedValue), $"Failed for '{value}'.TrimStart(null)");

        result = GlobalSession.Query.All<X>()
          .Where(x => x.FString == value && x.FString.TrimEnd(null) == value.TrimEnd(null)).Count();
        Assert.That(result, Is.EqualTo(expectedValue), $"Failed for '{value}'.TrimEnd(null)");

        result = GlobalSession.Query.All<X>()
          .Where(x => x.FString == value && x.FString.Trim(' ') == value.Trim(' ')).Count();
        Assert.That(result, Is.EqualTo(expectedValue), $"Failed for {value}.Trim(' ')");

        result = GlobalSession.Query.All<X>()
          .Where(x => x.FString == value && x.FString.TrimStart(' ') == value.TrimStart(' ')).Count();
        Assert.That(result, Is.EqualTo(expectedValue), $"Failed for {value}.TrimStart(' ')");

        result = GlobalSession.Query.All<X>()
          .Where(x => x.FString == value && x.FString.TrimEnd(' ') == value.TrimEnd(' ')).Count();
        Assert.That(result, Is.EqualTo(expectedValue), $"Failed for {value}.TrimEnd(' ')");
      }
    }

    [Test]
    public void TrimOtherCharTest()
    {
      Require.ProviderIsNot(
        StorageProvider.SqlServer | StorageProvider.SqlServerCe,
        "Can't trim anything except spaces");

      var results = GlobalSession.Query.All<X>()
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
    public void TrimOtherCharServerSideTest()
    {
      Require.ProviderIsNot(StorageProvider.SqlServer | StorageProvider.SqlServerCe);

      var checkForWhitespaceString = StorageProviderInfo.Instance.Provider.HasFlag(StorageProvider.Oracle)
        ? (emptyStringIsNull && whitespaceStringAsEmptyString) || autoTrimWhiteSpaces
        : emptyStringIsNull || autoTrimWhiteSpaces || whitespaceStringAsEmptyString;

      foreach (var value in testValues) {
        var expectedValue = value == StringOfWhiteSpaces && checkForWhitespaceString
          ? 2 : 1;

        var result = GlobalSession.Query.All<X>()
          .Where(x => x.FString == value && x.FString.TrimStart('P') == value.TrimStart('P')).Count();
        Assert.That(result, Is.EqualTo(expectedValue), $"Failed for '{value}'.TrimStart('P')");

        result = GlobalSession.Query.All<X>()
          .Where(x => x.FString == value && x.FString.Trim('o') == value.Trim('o')).Count();
        Assert.That(result, Is.EqualTo(expectedValue), $"Failed for '{value}'.Trim('o')");

        result = GlobalSession.Query.All<X>()
          .Where(x => x.FString == value && x.FString.TrimEnd(')') == value.TrimEnd(')')).Count();
        Assert.That(result, Is.EqualTo(expectedValue), $"Failed for '{value}'.TrimEnd(')')");
      }
    }

    [Test]
    public void TrimMultipleCharsTest()
    {
      Require.ProviderIsNot(
        StorageProvider.SqlServer | StorageProvider.Oracle | StorageProvider.SqlServerCe | StorageProvider.MySql,
        "No support for trimming multiple characters");

      var results = GlobalSession.Query.All<X>()
        .Select(x => new {
          String = x.FString,
          StringTrimLeadingZeroAndOne = x.FString.TrimStart('0', '1'),
        }).ToList();
      foreach (var x in results) {
        Assert.AreEqual(ConvertString(x.String).TrimStart('0', '1'), ConvertString(x.StringTrimLeadingZeroAndOne));
      }
    }

    [Test]
    public void TrimMultipleCharsServerSideTest()
    {
      Require.ProviderIsNot(
        StorageProvider.SqlServer | StorageProvider.Oracle | StorageProvider.SqlServerCe | StorageProvider.MySql,
        "No support for trimming multiple characters");

      foreach (var value in testValues) {
        var expectedValue = value == StringOfWhiteSpaces && (emptyStringIsNull || autoTrimWhiteSpaces || whitespaceStringAsEmptyString)
          ? 2 : 1;

        var result = GlobalSession.Query.All<X>()
          .Where(x => x.FString == value && x.FString.TrimStart('0', '1') == value.TrimStart('0', '1')).Count();
        Assert.That(result, Is.EqualTo(expectedValue), $"Failed for '{value}'.TrimStart('0', '1')");
      }
    }

    #endregion

    #region StartsWith, EndsWith, Contains

    [Test]
    public void StartsWithTest()
    {
      var result = GlobalSession.Query.All<X>().Select(x => new {
        x.Id,
        String = x.FString,
        StartsWithA = x.FString.StartsWith("A"),
        StartsWithPercent = x.FString.StartsWith("%"),
        StartsWithGround = x.FString.StartsWith("_"),
        StartsWithPrecentGround = x.FString.StartsWith("%_"),
        StartsWithGroundPercent = x.FString.StartsWith("_%"),
        StartsWithEscape = x.FString.StartsWith("^"),
        StartsWithOpenBracket = x.FString.StartsWith("["),
        StartsWithCloseBracket = x.FString.StartsWith("]"),
      }).ToList();
      foreach (var x in result) {
        Assert.AreEqual(ConvertString(x.String).StartsWith("A"), x.StartsWithA);
        Assert.AreEqual(ConvertString(x.String).StartsWith("%"), x.StartsWithPercent);
        Assert.AreEqual(ConvertString(x.String).StartsWith("_"), x.StartsWithGround);
        Assert.AreEqual(ConvertString(x.String).StartsWith("%_"), x.StartsWithPrecentGround);
        Assert.AreEqual(ConvertString(x.String).StartsWith("_%"), x.StartsWithGroundPercent);
        Assert.AreEqual(ConvertString(x.String).StartsWith("^"), x.StartsWithEscape);
        Assert.AreEqual(ConvertString(x.String).StartsWith("["), x.StartsWithOpenBracket);
        Assert.AreEqual(ConvertString(x.String).StartsWith("]"), x.StartsWithCloseBracket);
      }
    }

    [Test]
    public void StartsWithServerSideTest()
    {
      var checkForWhitespaceString = StorageProviderInfo.Instance.Provider.HasFlag(StorageProvider.Oracle)
        ? (emptyStringIsNull && whitespaceStringAsEmptyString) || autoTrimWhiteSpaces
        : emptyStringIsNull || autoTrimWhiteSpaces || whitespaceStringAsEmptyString;

      foreach (var value in testValues) {
        var expectedValue = value == StringOfWhiteSpaces && checkForWhitespaceString
          ? 2 : 1;

        var result = GlobalSession.Query.All<X>()
          .Where(x => x.FString == value && x.FString.StartsWith("A") == value.StartsWith("A")).Count();
        Assert.That(result, Is.EqualTo(expectedValue), $"Failed for '{value}'.StartsWith(\"A\")");

        result = GlobalSession.Query.All<X>()
          .Where(x => x.FString == value && x.FString.StartsWith("%") == value.StartsWith("%")).Count();
        Assert.That(result, Is.EqualTo(expectedValue), $"Failed for '{value}'.StartsWith(\"%\")");

        result = GlobalSession.Query.All<X>()
          .Where(x => x.FString == value && x.FString.StartsWith("_") == value.StartsWith("_")).Count();
        Assert.That(result, Is.EqualTo(expectedValue), $"Failed for '{value}'.StartsWith(\"_\")");

        result = GlobalSession.Query.All<X>()
          .Where(x => x.FString == value && x.FString.StartsWith("%_") == value.StartsWith("%_")).Count();
        Assert.That(result, Is.EqualTo(expectedValue), $"Failed for '{value}'.StartsWith(\"%_\")");

        result = GlobalSession.Query.All<X>()
          .Where(x => x.FString == value && x.FString.StartsWith("_%") == value.StartsWith("_%")).Count();
        Assert.That(result, Is.EqualTo(expectedValue), $"Failed for '{value}'.StartsWith(\"_%\")");

        result = GlobalSession.Query.All<X>()
          .Where(x => x.FString == value && x.FString.StartsWith("^") == value.StartsWith("^")).Count();
        Assert.That(result, Is.EqualTo(expectedValue), $"Failed for '{value}'.StartsWith(\"^\")");

        result = GlobalSession.Query.All<X>()
          .Where(x => x.FString == value && x.FString.StartsWith("[") == value.StartsWith("[")).Count();
        Assert.That(result, Is.EqualTo(expectedValue), $"Failed for '{value}'.StartsWith(\"[\")");

        result = GlobalSession.Query.All<X>()
          .Where(x => x.FString == value && x.FString.StartsWith("]") == value.StartsWith("]")).Count();
        Assert.That(result, Is.EqualTo(expectedValue), $"Failed for '{value}'.StartsWith(\"]\")");
      }
    }

    [Test]
    public void EndsWithTest()
    {
      var result = GlobalSession.Query.All<X>().Select(x => new {
        x.Id,
        String = x.FString,
        EndsWithA = x.FString.EndsWith("A"),
        EndsWithPercent = x.FString.EndsWith("%"),
        EndsWithGround = x.FString.EndsWith("_"),
        EndsWithPrecentGround = x.FString.EndsWith("%_"),
        EndsWithGroundPercent = x.FString.EndsWith("_%"),
        EndsWithEscape = x.FString.EndsWith("^"),
        EndsWithOpenBracket = x.FString.EndsWith("["),
        EndsWithCloseBracket = x.FString.EndsWith("]"),
      }).ToList();
      foreach (var x in result) {
        Assert.AreEqual(ConvertString(x.String).EndsWith("A"), x.EndsWithA);
        Assert.AreEqual(ConvertString(x.String).EndsWith("%"), x.EndsWithPercent);
        Assert.AreEqual(ConvertString(x.String).EndsWith("_"), x.EndsWithGround);
        Assert.AreEqual(ConvertString(x.String).EndsWith("%_"), x.EndsWithPrecentGround);
        Assert.AreEqual(ConvertString(x.String).EndsWith("_%"), x.EndsWithGroundPercent);
        Assert.AreEqual(ConvertString(x.String).EndsWith("^"), x.EndsWithEscape);
        Assert.AreEqual(ConvertString(x.String).EndsWith("["), x.EndsWithOpenBracket);
        Assert.AreEqual(ConvertString(x.String).EndsWith("]"), x.EndsWithCloseBracket);
      }
    }

    [Test]
    public void EndsWithServerSideTest()
    {
      var checkForWhitespaceString = StorageProviderInfo.Instance.Provider.HasFlag(StorageProvider.Oracle)
        ? (emptyStringIsNull && whitespaceStringAsEmptyString) || autoTrimWhiteSpaces
        : emptyStringIsNull || autoTrimWhiteSpaces || whitespaceStringAsEmptyString;

      foreach (var value in testValues) {
        var expectedValue = value == StringOfWhiteSpaces && checkForWhitespaceString
          ? 2 : 1;

        var result = GlobalSession.Query.All<X>()
          .Where(x => x.FString == value && x.FString.EndsWith("A") == value.EndsWith("A")).Count();
        Assert.That(result, Is.EqualTo(expectedValue), $"Failed for '{value}'.StartsWith(\"A\")");

        result = GlobalSession.Query.All<X>()
          .Where(x => x.FString == value && x.FString.EndsWith("%") == value.EndsWith("%")).Count();
        Assert.That(result, Is.EqualTo(expectedValue), $"Failed for '{value}'.StartsWith(\"%\")");

        result = GlobalSession.Query.All<X>()
          .Where(x => x.FString == value && x.FString.EndsWith("_") == value.EndsWith("_")).Count();
        Assert.That(result, Is.EqualTo(expectedValue), $"Failed for '{value}'.StartsWith(\"_\")");

        result = GlobalSession.Query.All<X>()
          .Where(x => x.FString == value && x.FString.EndsWith("%_") == value.EndsWith("%_")).Count();
        Assert.That(result, Is.EqualTo(expectedValue), $"Failed for '{value}'.StartsWith(\"%_\")");

        result = GlobalSession.Query.All<X>()
          .Where(x => x.FString == value && x.FString.EndsWith("_%") == value.EndsWith("_%")).Count();
        Assert.That(result, Is.EqualTo(expectedValue), $"Failed for '{value}'.StartsWith(\"_%\")");

        result = GlobalSession.Query.All<X>()
          .Where(x => x.FString == value && x.FString.EndsWith("^") == value.EndsWith("^")).Count();
        Assert.That(result, Is.EqualTo(expectedValue), $"Failed for '{value}'.StartsWith(\"^\")");

        result = GlobalSession.Query.All<X>()
          .Where(x => x.FString == value && x.FString.EndsWith("[") == value.EndsWith("[")).Count();
        Assert.That(result, Is.EqualTo(expectedValue), $"Failed for '{value}'.StartsWith(\"[\")");

        result = GlobalSession.Query.All<X>()
          .Where(x => x.FString == value && x.FString.EndsWith("]") == value.EndsWith("]")).Count();
        Assert.That(result, Is.EqualTo(expectedValue), $"Failed for '{value}'.StartsWith(\"]\")");
      }
    }

    [Test]
    public void ContainsTest()
    {
      var result = GlobalSession.Query.All<X>().Select(x => new {
        x.Id,
        String = x.FString,
        ContainsA = x.FString.Contains("A"),
        ContainsPercent = x.FString.Contains("%"),
        ContainsGround = x.FString.Contains("_"),
        ContainsPrecentGround = x.FString.Contains("%_"),
        ContainsGroundPercent = x.FString.Contains("_%"),
        ContainsEscape = x.FString.Contains("^"),
        ContainsOpenBracket = x.FString.Contains("["),
        ContainsCloseBracket = x.FString.Contains("]"),
      }).ToList();
      foreach (var x in result) {
        Assert.AreEqual(ConvertString(x.String).Contains("A"), x.ContainsA);
        Assert.AreEqual(ConvertString(x.String).Contains("%"), x.ContainsPercent);
        Assert.AreEqual(ConvertString(x.String).Contains("_"), x.ContainsGround);
        Assert.AreEqual(ConvertString(x.String).Contains("%_"), x.ContainsPrecentGround);
        Assert.AreEqual(ConvertString(x.String).Contains("_%"), x.ContainsGroundPercent);
        Assert.AreEqual(ConvertString(x.String).Contains("^"), x.ContainsEscape);
        Assert.AreEqual(ConvertString(x.String).Contains("["), x.ContainsOpenBracket);
        Assert.AreEqual(ConvertString(x.String).Contains("]"), x.ContainsCloseBracket);
      }
    }

    [Test]
    public void ContainsServerSideTest()
    {
      var checkForWhitespaceString = StorageProviderInfo.Instance.Provider.HasFlag(StorageProvider.Oracle)
        ? (emptyStringIsNull && whitespaceStringAsEmptyString) || autoTrimWhiteSpaces
        : emptyStringIsNull || autoTrimWhiteSpaces || whitespaceStringAsEmptyString;

      foreach (var value in testValues) {
        var expectedValue = value == StringOfWhiteSpaces && checkForWhitespaceString
          ? 2 : 1;

        var result = GlobalSession.Query.All<X>()
          .Where(x => x.FString == value && x.FString.Contains("A") == value.Contains("A")).Count();
        Assert.That(result, Is.EqualTo(expectedValue), $"Failed for '{value}'.StartsWith(\"A\")");

        result = GlobalSession.Query.All<X>()
          .Where(x => x.FString == value && x.FString.Contains("%") == value.Contains("%")).Count();
        Assert.That(result, Is.EqualTo(expectedValue), $"Failed for '{value}'.StartsWith(\"%\")");

        result = GlobalSession.Query.All<X>()
          .Where(x => x.FString == value && x.FString.Contains("_") == value.Contains("_")).Count();
        Assert.That(result, Is.EqualTo(expectedValue), $"Failed for '{value}'.StartsWith(\"_\")");

        result = GlobalSession.Query.All<X>()
          .Where(x => x.FString == value && x.FString.Contains("%_") == value.Contains("%_")).Count();
        Assert.That(result, Is.EqualTo(expectedValue), $"Failed for '{value}'.StartsWith(\"%_\")");

        result = GlobalSession.Query.All<X>()
          .Where(x => x.FString == value && x.FString.Contains("_%") == value.Contains("_%")).Count();
        Assert.That(result, Is.EqualTo(expectedValue), $"Failed for '{value}'.StartsWith(\"_%\")");

        result = GlobalSession.Query.All<X>()
          .Where(x => x.FString == value && x.FString.Contains("^") == value.Contains("^")).Count();
        Assert.That(result, Is.EqualTo(expectedValue), $"Failed for '{value}'.StartsWith(\"^\")");

        result = GlobalSession.Query.All<X>()
          .Where(x => x.FString == value && x.FString.Contains("[") == value.Contains("[")).Count();
        Assert.That(result, Is.EqualTo(expectedValue), $"Failed for '{value}'.StartsWith(\"[\")");

        result = GlobalSession.Query.All<X>()
          .Where(x => x.FString == value && x.FString.EndsWith("]") == value.EndsWith("]")).Count();
        Assert.That(result, Is.EqualTo(expectedValue), $"Failed for '{value}'.StartsWith(\"]\")");
      }
    }

    #endregion

    #region PadLeft, PadRight

    [Test]
    public void PaddingTest()
    {
      var result = GlobalSession.Query.All<X>().Select(x => new {
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

    [Test]
    public void PaddingServerSideTest()
    {
      Require.ProviderIsNot(StorageProvider.Firebird);

      var checkForWhitespaceString = StorageProviderInfo.Instance.Provider.HasFlag(StorageProvider.Oracle)
        ? (emptyStringIsNull && whitespaceStringAsEmptyString) || autoTrimWhiteSpaces
        : emptyStringIsNull || autoTrimWhiteSpaces || whitespaceStringAsEmptyString;

      foreach (var value in testValues) {
        var expectedValue = value == StringOfWhiteSpaces && checkForWhitespaceString
          ? 2 : 1;

        var result = GlobalSession.Query.All<X>()
          .Where(x => x.FString == value && x.FString.PadLeft(10) == value.PadLeft(10)).Count();
        Assert.That(result, Is.EqualTo(expectedValue), $"Failed for '{value}'.PadLeft(10)");

        result = GlobalSession.Query.All<X>()
          .Where(x => x.FString == value && x.FString.PadRight(10) == value.PadRight(10)).Count();
        Assert.That(result, Is.EqualTo(expectedValue), $"Failed for '{value}'.PadRight(10)");

        result = GlobalSession.Query.All<X>()
          .Where(x => x.FString == value && x.FString.PadLeft(10, 'X') == value.PadLeft(10, 'X')).Count();
        Assert.That(result, Is.EqualTo(1), $"Failed for '{value}'.PadLeft(10, 'X')");

        result = GlobalSession.Query.All<X>()
          .Where(x => x.FString == value && x.FString.PadRight(10, 'X') == value.PadRight(10, 'X')).Count();
        Assert.That(result, Is.EqualTo(1), $"Failed for '{value}'.PadRight(10, 'X')");
      }
    }

    [Test]
    public void PaddingServerSideFirebirdTest()
    {
      Require.ProviderIs(StorageProvider.Firebird, "Cuts-off length of result string if it is bigger than endlength in LPAD/RPAD");

      foreach (var value in testValues.Where(s => s != StringOfWhiteSpaces)) {
        var result = GlobalSession.Query.All<X>()
          .Where(x => x.FString == value && x.FString.PadLeft(10) == value.PadLeft(10).Substring(0, 10)).Count();
        Assert.That(result, Is.EqualTo(1), $"Failed for '{value}'.PadLeft(10)");

        result = GlobalSession.Query.All<X>()
          .Where(x => x.FString == value && x.FString.PadRight(10) == value.PadRight(10).Substring(0, 10)).Count();
        Assert.That(result, Is.EqualTo(1), $"Failed for '{value}'.PadRight(10)");

        result = GlobalSession.Query.All<X>()
          .Where(x => x.FString == value && x.FString.PadLeft(10, 'X') == value.PadLeft(10, 'X').Substring(0, 10)).Count();
        Assert.That(result, Is.EqualTo(1), $"Failed for '{value}'.PadLeft(10, 'X')");

        result = GlobalSession.Query.All<X>()
          .Where(x => x.FString == value && x.FString.PadRight(10, 'X') == value.PadRight(10, 'X').Substring(0, 10)).Count();
        Assert.That(result, Is.EqualTo(1), $"Failed for '{value}'.PadRight(10, 'X')");
      }

      foreach (var value in testValues.Where(s => s == StringOfWhiteSpaces)) {
        var result = GlobalSession.Query.All<X>()
          .Where(x => x.FString == value && x.FString.PadLeft(10) == string.Empty.PadLeft(10)).Count();
        Assert.That(result, Is.EqualTo(2), $"Failed for '{value}'.PadLeft(10)");

        result = GlobalSession.Query.All<X>()
          .Where(x => x.FString == value && x.FString.PadRight(10) == string.Empty.PadRight(10)).Count();
        Assert.That(result, Is.EqualTo(2), $"Failed for '{value}'.PadRight(10)");

        result = GlobalSession.Query.All<X>()
          .Where(x => x.FString == value && x.FString.PadLeft(10, 'X') == string.Empty.PadLeft(10, 'X')).Count();
        Assert.That(result, Is.EqualTo(1), $"Failed for '{value}'.PadLeft(10, 'X')");

        result = GlobalSession.Query.All<X>()
          .Where(x => x.FString == value && x.FString.PadRight(10, 'X') == string.Empty.PadRight(10, 'X')).Count();
        Assert.That(result, Is.EqualTo(1), $"Failed for '{value}'.PadRight(10, 'X')");
      }
    }

    #endregion

    #region IndexOf

    [Test]
    public void IndexOfTest()
    {
      var comparison = StorageProviderInfo.Instance.Provider switch {
        StorageProvider.MySql => StringComparison.InvariantCultureIgnoreCase,
        StorageProvider.SqlServer => StringComparison.InvariantCultureIgnoreCase,
        _ => StringComparison.InvariantCulture
      };

      var _char = 'o';
      var baseQuery = GlobalSession.Query.All<X>().Where(x => x.FString != string.Empty);
      if (emptyStringIsNull || autoTrimWhiteSpaces) {
        baseQuery = baseQuery.Where(x => x.FString != null);
      }

      var results = baseQuery
          .Select(c => new {
            String = c.FString,
            IndexOfChar = c.FString.IndexOf(_char),
            IndexOfCharStart = c.FString.IndexOf(_char, 1),
            IndexOfCharStartCount = c.FString.IndexOf(_char, 1, 1),
            IndexOfString = c.FString.IndexOf(_char.ToString()),
            IndexOfStringStart = c.FString.IndexOf(_char.ToString(), 1),
            IndexOfStringStartCount = c.FString.IndexOf(_char.ToString(), 1, 1)
          })
          .ToList();
      foreach (var x in results) {
        Assert.AreEqual(ConvertString(x.String).IndexOf(_char), x.IndexOfChar);
        Assert.AreEqual(ConvertString(x.String).IndexOf(_char, 1), x.IndexOfCharStart);
        Assert.AreEqual(ConvertString(x.String).IndexOf(_char, 1, 1), x.IndexOfCharStartCount);
        Assert.AreEqual(ConvertString(x.String).IndexOf(_char.ToString()), x.IndexOfString);
        Assert.AreEqual(ConvertString(x.String).IndexOf(_char.ToString(), 1), x.IndexOfStringStart);
        Assert.AreEqual(ConvertString(x.String).IndexOf(_char.ToString(), 1, 1), x.IndexOfStringStartCount);
      }
    }

    [Test]
    public void IndexOfServerSideTest()
    {
      Require.ProviderIsNot(StorageProvider.Sqlite, "No support for Position operation.");

      var comparison = StorageProviderInfo.Instance.Provider switch {
        StorageProvider.MySql => StringComparison.InvariantCultureIgnoreCase,
        StorageProvider.SqlServer => StringComparison.InvariantCultureIgnoreCase,
        _ => StringComparison.InvariantCulture
      };

      var _char = 'o';
      foreach (var value in testValues) {
        var expectedValue = value == StringOfWhiteSpaces && whitespaceStringAsEmptyString
          ? 2 : 1;

        var result = GlobalSession.Query.All<X>()
          .Where(x => x.FString == value && x.FString.IndexOf(_char) == value.IndexOf(_char.ToString(), comparison)).Count();
        Assert.That(result, Is.EqualTo(expectedValue), $"Failed for '{value}'.IndexOf(_char)");

        result = GlobalSession.Query.All<X>()
          .Where(x => x.FString == value && x.FString.IndexOf(_char, 1) == value.IndexOf(_char.ToString(), 1, comparison)).Count();
        Assert.That(result, Is.EqualTo(expectedValue), $"Failed for '{value}'.IndexOf(_char, 1)");

        result = GlobalSession.Query.All<X>()
          .Where(x => x.FString == value && x.FString.IndexOf(_char, 1, 1) == value.IndexOf(_char.ToString(), 1, 1, comparison)).Count();
        Assert.That(result, Is.EqualTo(expectedValue), $"Failed for '{value}'.TrimStart('0', '1')");

        result = GlobalSession.Query.All<X>()
          .Where(x => x.FString == value && x.FString.IndexOf(_char.ToString()) == value.IndexOf(_char.ToString(), comparison)).Count();
        Assert.That(result, Is.EqualTo(expectedValue), $"Failed for '{value}'.IndexOf(_char.ToString())");

        result = GlobalSession.Query.All<X>()
          .Where(x => x.FString == value && x.FString.IndexOf(_char.ToString(), 1) == value.IndexOf(_char.ToString(), 1, comparison)).Count();
        Assert.That(result, Is.EqualTo(expectedValue), $"Failed for '{value}'.IndexOf(_char.ToString(), 1)");

        result = GlobalSession.Query.All<X>()
          .Where(x => x.FString == value && x.FString.IndexOf(_char.ToString(), 1, 1) == value.IndexOf(_char.ToString(), 1, 1, comparison)).Count();
        Assert.That(result, Is.EqualTo(expectedValue), $"Failed for '{value}'.IndexOf(_char.ToString(), 1, 1)");
      }
    }

    [Test]
    public void IndexOfSqliteServerSideTest()
    {
      Require.ProviderIs(StorageProvider.Sqlite);

      var _char = 'o';
      var exception = Assert.Throws<QueryTranslationException>(() =>
        GlobalSession.Query.All<X>()
          .Where(x => x.FString.IndexOf(_char) > 0)
          .ToList());
      Assert.That(exception.InnerException, Is.InstanceOf<NotSupportedException>());
    }

    #endregion

    private string ConvertString(string value) =>
      emptyStringIsNull && value == null ? string.Empty : value;
  }
}