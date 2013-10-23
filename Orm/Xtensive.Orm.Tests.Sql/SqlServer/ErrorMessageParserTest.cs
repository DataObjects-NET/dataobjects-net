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
    [Test]
    public void ExtractQuotedTest()
    {
      Assert.AreEqual("hello", ErrorMessageParser.ExtractQuotedText("hello"));
      Assert.AreEqual("hello", ErrorMessageParser.ExtractQuotedText("\"hello\""));
      Assert.AreEqual("hello", ErrorMessageParser.ExtractQuotedText("!!\"hello\""));
      Assert.AreEqual("hello", ErrorMessageParser.ExtractQuotedText("\"hello\"!!"));
      Assert.AreEqual("hello", ErrorMessageParser.ExtractQuotedText("!!\"hello\"!!"));
      Assert.AreEqual(string.Empty, ErrorMessageParser.ExtractQuotedText("\"\"!!"));
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
      Assert.AreEqual(3, result.Count);
      Assert.AreEqual("col", result[1]);
      Assert.AreEqual("Table_1", ErrorMessageParser.CutDatabaseAndSchemaPrefix(result[2]));
      Assert.AreEqual("INSERT", result[3]);

      const string violationOfForeignKey = "The INSERT statement conflicted with the FOREIGN KEY constraint \"FK_Table_2_Table_1\". The conflict occurred in database \"DO40-Tests\", table \"dbo.Table_1\", column 'col'.";
      result = parser.Parse(2, violationOfForeignKey);
      Assert.AreEqual(6, result.Count);
      Assert.AreEqual("INSERT", result[1]);
      Assert.AreEqual("FOREIGN KEY", result[2]);
      Assert.AreEqual("FK_Table_2_Table_1", result[3]);
      Assert.AreEqual("DO40-Tests", result[4]);
      Assert.AreEqual("Table_1", ErrorMessageParser.CutSchemaPrefix(result[5]));
      Assert.AreEqual("col", ErrorMessageParser.ExtractQuotedText(result[6]));

      const string canNotInsertDuplicateKey = "Cannot insert duplicate key row in object 'dbo.Table_1' with unique index 'Test'. The duplicate key value is (1).";
      result = parser.Parse(3, canNotInsertDuplicateKey);
      Assert.AreEqual(3, result.Count);
      Assert.AreEqual("Table_1", ErrorMessageParser.CutSchemaPrefix(result[1]));
      Assert.AreEqual("Test", result[2]);
      Assert.AreEqual("(1)", result[3]);

      const string violationOfPrimaryKey = "Violation of PRIMARY KEY constraint 'PK_Table_1'. Cannot insert duplicate key in object 'dbo.Table_1'. The duplicate key value is (1).";
      result = parser.Parse(4, violationOfPrimaryKey);
      Assert.AreEqual(4, result.Count);
      Assert.AreEqual("PRIMARY KEY", result[1]);
      Assert.AreEqual("PK_Table_1", result[2]);
      Assert.AreEqual("Table_1", ErrorMessageParser.CutSchemaPrefix(result[3]));
      Assert.AreEqual("(1)", result[4]);
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
      Assert.AreEqual(expected.Length, actual.Count);
      for (int i = 0; i<expected.Length; i++) {
        Assert.AreEqual(expected[i], actual[i + 1]);
      }
    }

    private ErrorMessageParser CreateParser(bool isEnglish, params string[] templates)
    {
      return new ErrorMessageParser(templates.Select((template, index) => new KeyValuePair<int, string>(index + 1, template)), isEnglish);
    }
  }
}