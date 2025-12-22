// Copyright (C) 2011 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2011.09.28

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using NUnit.Framework;
using Xtensive.Sql.Drivers.SqlServer;

namespace Xtensive.Orm.Tests.Sql.SqlServer
{
  [TestFixture]
  public class ErrorMessageParserTest
  {
    [OneTimeSetUp]
    protected virtual void SetUp()
    {
      CheckRequirements();
    }

    protected virtual void CheckRequirements()
    {
      Require.ProviderIs(StorageProvider.SqlServer);
    }

    [Test]
    public void ExtractQuotedTest()
    {
      Assert.That(ErrorMessageParser.ExtractQuotedText("hello"), Is.EqualTo("hello"));
      Assert.That(ErrorMessageParser.ExtractQuotedText("\"hello\""), Is.EqualTo("hello"));
      Assert.That(ErrorMessageParser.ExtractQuotedText("!!\"hello\""), Is.EqualTo("hello"));
      Assert.That(ErrorMessageParser.ExtractQuotedText("\"hello\"!!"), Is.EqualTo("hello"));
      Assert.That(ErrorMessageParser.ExtractQuotedText("!!\"hello\"!!"), Is.EqualTo("hello"));
      Assert.That(ErrorMessageParser.ExtractQuotedText("\"\"!!"), Is.EqualTo(string.Empty));
    }

    [Test]
    public void RealMessagesTest()
    {
      var parser = CreateParser(true,
        "Cannot insert the value NULL into column '%.*ls', table '%.*ls'; column does not allow nulls. %ls fails.",
        "The %ls statement conflicted with the %ls constraint \"%.*ls\". The conflict occurred in database \"%.*ls\", table \"%.*ls\"%ls%.*ls%ls.",
        "Cannot insert duplicate key row in object '%.*ls' with unique index '%.*ls'. The duplicate key value is %ls.",
        "Violation of %ls constraint '%.*ls'. Cannot insert duplicate key in object '%.*ls'. The duplicate key value is %ls.");

      const string canNotInsertNull = "Cannot insert the value NULL into column 'col', table 'DO40-Tests.dbo.Table_1'; column does not allow nulls. INSERT fails.";
      var result = parser.Parse(1, canNotInsertNull);
      Assert.That(result.Count, Is.EqualTo(3));
      Assert.That(result[1], Is.EqualTo("col"));
      Assert.That(ErrorMessageParser.CutDatabaseAndSchemaPrefix(result[2]), Is.EqualTo("Table_1"));
      Assert.That(result[3], Is.EqualTo("INSERT"));

      const string violationOfForeignKey = "The INSERT statement conflicted with the FOREIGN KEY constraint \"FK_Table_2_Table_1\". The conflict occurred in database \"DO40-Tests\", table \"dbo.Table_1\", column 'col'.";
      result = parser.Parse(2, violationOfForeignKey);
      Assert.That(result.Count, Is.EqualTo(6));
      Assert.That(result[1], Is.EqualTo("INSERT"));
      Assert.That(result[2], Is.EqualTo("FOREIGN KEY"));
      Assert.That(result[3], Is.EqualTo("FK_Table_2_Table_1"));
      Assert.That(result[4], Is.EqualTo("DO40-Tests"));
      Assert.That(ErrorMessageParser.CutSchemaPrefix(result[5]), Is.EqualTo("Table_1"));
      Assert.That(ErrorMessageParser.ExtractQuotedText(result[6]), Is.EqualTo("col"));

      const string canNotInsertDuplicateKey = "Cannot insert duplicate key row in object 'dbo.Table_1' with unique index 'Test'. The duplicate key value is (1).";
      result = parser.Parse(3, canNotInsertDuplicateKey);
      Assert.That(result.Count, Is.EqualTo(3));
      Assert.That(ErrorMessageParser.CutSchemaPrefix(result[1]), Is.EqualTo("Table_1"));
      Assert.That(result[2], Is.EqualTo("Test"));
      Assert.That(result[3], Is.EqualTo("(1)"));

      const string violationOfPrimaryKey = "Violation of PRIMARY KEY constraint 'PK_Table_1'. Cannot insert duplicate key in object 'dbo.Table_1'. The duplicate key value is (1).";
      result = parser.Parse(4, violationOfPrimaryKey);
      Assert.That(result.Count, Is.EqualTo(4));
      Assert.That(result[1], Is.EqualTo("PRIMARY KEY"));
      Assert.That(result[2], Is.EqualTo("PK_Table_1"));
      Assert.That(ErrorMessageParser.CutSchemaPrefix(result[3]), Is.EqualTo("Table_1"));
      Assert.That(result[4], Is.EqualTo("(1)"));
    }

    [Test]
    public void SynteticEnglishTest()
    {
      var parser = CreateParser(true, "%ls", "AAA%lsBBB", "%.*lsHELLO", "BYE%ls", "A%.*lsB%.*lsC%.*ls", "%ls%ls%ls%ls");
      RunAllTests(parser);
    }

    [Test]
    public void SynteticNonEnglishTest()
    {
      var parser = CreateParser(false, "%1!", "AAA%1!BBB", "%1!HELLO", "BYE%1!", "A%1!B%2!C%3!", "%1!%2!%3!");
      RunAllTests(parser);
    }

    private void RunAllTests(ErrorMessageParser parser)
    {
      RunTest(parser, 1, "whatever", "whatever");
      RunTest(parser, 2, "AAA??BBB", "??");
      RunTest(parser, 3, "'aaa'HELLO", "'aaa'");
      RunTest(parser, 4, "BYE123", "123");
      RunTest(parser, 5, "A1B2C3", "1", "2", "3");
      RunTest(parser, 6, "~", "~");
    }

    private void RunTest(ErrorMessageParser parser, int code, string input, params string[] expected)
    {
      var actual = parser.Parse(code, input);
      Assert.That(actual.Count, Is.EqualTo(expected.Length));
      for (int i = 0; i < expected.Length; i++) {
        Assert.That(actual[i + 1], Is.EqualTo(expected[i]));
      }
    }

    private ErrorMessageParser CreateParser(bool isEnglish, params string[] templates)
    {
      return new ErrorMessageParser(templates.Select((template, index) => new KeyValuePair<int, string>(index + 1, template)), isEnglish);
    }
  }
}