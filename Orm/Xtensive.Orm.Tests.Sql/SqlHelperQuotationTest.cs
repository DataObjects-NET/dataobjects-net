// Copyright (C) 2020 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kulakov
// Created:    2020.04.23

using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;
using Xtensive.Sql;

namespace Xtensive.Orm.Tests.Sql
{
  [TestFixture]
  public sealed class SqlHelperQuotationTest
  {
    public static IEnumerable<(string[] testNames, string expectedResult)> QuotesRegularNames()
    {
      yield return (new string[] { "name1" }, "\"name1\"");
      yield return (new string[] { "n\"\"\"1" }, "\"n\"\"\"\"\"\"1\"");
      yield return (new string[] { "name1", "name2" }, "\"name1\".\"name2\"");
      yield return (new string[] { "n\"\"\"1", "n\"\"\"2" }, "\"n\"\"\"\"\"\"1\".\"n\"\"\"\"\"\"2\"");
      yield return (new string[] { "name1", "name2", "name3" }, "\"name1\".\"name2\".\"name3\"");
      yield return (new string[] { "n\"\"\"1", "n\"\"\"2", "n\"\"\"3" }, "\"n\"\"\"\"\"\"1\".\"n\"\"\"\"\"\"2\".\"n\"\"\"\"\"\"3\"");
    }

    public static IEnumerable<(string[] testNames, string expectedResult)> QuotesEmptyNames()
    {
      yield return (new string[] {"name1", "name2", ""}, "\"name1\".\"name2\"");
      yield return (new string[] { "name1", "", "name3" }, "\"name1\".\"name3\"");
      yield return (new string[] { "", "name2", "name3" }, "\"name2\".\"name3\"");
      yield return (new string[] { "name1", "", "" } , "\"name1\"");
      yield return (new string[] { "", "", "name3" }, "\"name3\"");
      yield return (new string[] { "", "name2", "" }, "\"name2\"");
      yield return (new string[] { "", "", "" }, string.Empty);
    }

    public static IEnumerable<(string[] testNames, string expectedResult)> QuotesNullNames()
    {
      yield return (new string[] { "name1", "name2", null }, "\"name1\".\"name2\"");
      yield return (new string[] { "name1", null, "name3" }, "\"name1\".\"name3\"");
      yield return (new string[] { null, "name2", "name3" }, "\"name2\".\"name3\"");
      yield return (new string[] { "name1", null, null }, "\"name1\"");
      yield return (new string[] { null, null, "name3" }, "\"name3\"");
      yield return (new string[] { null, "name2", null }, "\"name2\"");
      yield return (new string[] { null, null, null }, string.Empty);
    }

    public static IEnumerable<(string[] testNames, string expectedResult)> BracketsRegularNames()
    {
      yield return (new string[] { "name1" }, "[name1]");
      yield return (new string[] { "n[[[1" }, "[n[[[1]");
      yield return (new string[] { "n]]]1" }, "[n]]]]]]1]");
      yield return (new string[] { "name1", "name2" }, "[name1].[name2]");
      yield return (new string[] { "n[[[1", "n[[[2" }, "[n[[[1].[n[[[2]");
      yield return (new string[] { "n]]]1", "n]]]2" }, "[n]]]]]]1].[n]]]]]]2]");
      yield return (new string[] { "name1", "name2", "name3" }, "[name1].[name2].[name3]");
      yield return (new string[] { "n[[[1", "n[[[2", "n[[[3" }, "[n[[[1].[n[[[2].[n[[[3]");
      yield return (new string[] { "n]]]1", "n]]]2", "n]]]3" }, "[n]]]]]]1].[n]]]]]]2].[n]]]]]]3]");
    }

    public static IEnumerable<(string[] testNames, string expectedResult)> BracketsEmptyNames()
    {
      yield return (new string[] { "name1", "name2", "" }, "[name1].[name2]");
      yield return (new string[] { "name1", "", "name3" }, "[name1].[name3]");
      yield return (new string[] { "", "name2", "name3" }, "[name2].[name3]");
      yield return (new string[] { "name1", "", "" }, "[name1]");
      yield return (new string[] { "", "", "name3" }, "[name3]");
      yield return (new string[] { "", "name2", "" }, "[name2]");
      yield return (new string[] { "", "", "" }, string.Empty);
    }

    public static IEnumerable<(string[] testNames, string expectedResult)> BracketsNullNames()
    {
      yield return (new string[] { "name1", "name2", null }, "[name1].[name2]");
      yield return (new string[] { "name1", null, "name3" }, "[name1].[name3]");
      yield return (new string[] { null, "name2", "name3" }, "[name2].[name3]");
      yield return (new string[] { "name1", null, null }, "[name1]");
      yield return (new string[] { null, null, "name3" }, "[name3]");
      yield return (new string[] { null, "name2", null }, "[name2]");
      yield return (new string[] { null, null, null }, string.Empty);
    }

    public static IEnumerable<(string[] testNames, string expectedResult)> BackTickRegularNames()
    {
      yield return (new string[] { "name1" }, "`name1`");
      yield return (new string[] { "n```1" }, "`n``````1`");
      yield return (new string[] { "name1", "name2" }, "`name1`.`name2`");
      yield return (new string[] { "n```1", "n```2" }, "`n``````1`.`n``````2`");
      yield return (new string[] { "name1", "name2", "name3" }, "`name1`.`name2`.`name3`");
      yield return (new string[] { "n```1", "n```2", "n```3" }, "`n``````1`.`n``````2`.`n``````3`");
    }

    public static IEnumerable<(string[] testNames, string expectedResult)> BackTickEmptyNames()
    {
      yield return (new string[] { "name1", "name2", "" }, "`name1`.`name2`");
      yield return (new string[] { "name1", "", "name3" }, "`name1`.`name3`");
      yield return (new string[] { "", "name2", "name3" }, "`name2`.`name3`");
      yield return (new string[] { "name1", "", "" }, "`name1`");
      yield return (new string[] { "", "", "name3" }, "`name3`");
      yield return (new string[] { "", "name2", "" }, "`name2`");
      yield return (new string[] { "", "", "" }, string.Empty);
    }

    public static IEnumerable<(string[] testNames, string expectedResult)> BackTickNullNames()
    {
      yield return (new string[] { "name1", "name2", null }, "`name1`.`name2`");
      yield return (new string[] { "name1", null, "name3" }, "`name1`.`name3`");
      yield return (new string[] { null, "name2", "name3" }, "`name2`.`name3`");
      yield return (new string[] { "name1", null, null }, "`name1`");
      yield return (new string[] { null, null, "name3" }, "`name3`");
      yield return (new string[] { null, "name2", null }, "`name2`");
      yield return (new string[] { null, null, null }, string.Empty);
    }

    [Test]
    [TestCaseSource(nameof(SqlHelperQuotationTest.QuotesRegularNames))]
    public void QuoteIndentifierWithQuotesWithRegularNames((string[] testNames, string expectedResult) testData)
      => Assert.That(SqlHelper.QuoteIdentifierWithQuotes(testData.testNames),
        Is.EqualTo(testData.expectedResult));

    [Test]
    [TestCaseSource(nameof(SqlHelperQuotationTest.QuotesEmptyNames))]
    public void QuoteIndentifierWithQuotesWithEmptyNames((string[] testNames, string expectedResult) testData)
      => Assert.That(SqlHelper.QuoteIdentifierWithQuotes(testData.testNames),
        Is.EqualTo(testData.expectedResult));

    [Test]
    [TestCaseSource(nameof(SqlHelperQuotationTest.QuotesNullNames))]
    public void QuoteIndentifierWithQuotesWithNullNames((string[] testNames, string expectedResult) testData)
      => Assert.That(SqlHelper.QuoteIdentifierWithQuotes(testData.testNames),
        Is.EqualTo(testData.expectedResult));

    [Test]
    [TestCaseSource(nameof(SqlHelperQuotationTest.BracketsRegularNames))]
    public void QuoteIdentifierWithBracketsWithRegularNames((string[] testNames, string expectedResult) testData)
      => Assert.That(SqlHelper.QuoteIdentifierWithBrackets(testData.testNames),
        Is.EqualTo(testData.expectedResult));

    [Test]
    [TestCaseSource(nameof(SqlHelperQuotationTest.BracketsEmptyNames))]
    public void QuoteIdentifierWithBracketsWithEmptyNames((string[] testNames, string expectedResult) testData)
      => Assert.That(SqlHelper.QuoteIdentifierWithBrackets(testData.testNames),
        Is.EqualTo(testData.expectedResult));

    [Test]
    [TestCaseSource(nameof(SqlHelperQuotationTest.BracketsNullNames))]
    public void QuoteIdentifierWithBracketsWithNullNames((string[] testNames, string expectedResult) testData)
      => Assert.That(SqlHelper.QuoteIdentifierWithBrackets(testData.testNames),
        Is.EqualTo(testData.expectedResult));

    [Test]
    [TestCaseSource(nameof(SqlHelperQuotationTest.BackTickRegularNames))]
    public void QuoteIdentifierWithBackTicksWithRegularNames((string[] testNames, string expectedResult) testData)
      => Assert.That(SqlHelper.QuoteIdentifierWithBackTick(testData.testNames),
        Is.EqualTo(testData.expectedResult));

    [Test]
    [TestCaseSource(nameof(SqlHelperQuotationTest.BackTickEmptyNames))]
    public void QuoteIdentifierWithBackTicksWithEmptyNames((string[] testNames, string expectedResult) testData)
      => Assert.That(SqlHelper.QuoteIdentifierWithBackTick(testData.testNames),
        Is.EqualTo(testData.expectedResult));

    [Test]
    [TestCaseSource(nameof(SqlHelperQuotationTest.BackTickNullNames))]
    public void QuoteIdentifierWithBackTicksWithNullNames((string[] testNames, string expectedResult) testData)
      => Assert.That(SqlHelper.QuoteIdentifierWithBackTick(testData.testNames),
        Is.EqualTo(testData.expectedResult));
  }
}
