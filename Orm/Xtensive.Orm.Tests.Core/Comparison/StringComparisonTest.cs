// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexis Kochetov
// Created:    2009.05.08

using System;
using System.Globalization;
using NUnit.Framework;

namespace Xtensive.Orm.Tests.Core.Comparison
{
  [TestFixture]
  public class StringComparisonTest
  {
    [Ignore("")]
    [Test]
    public void StringCompare()
    {
      //      var x = "X" + char.MaxValue + char.MaxValue;
      string x = "X" + (char) 0xDBFF + (char) 0xDFFF;
      string y = "X";

      string a = "" + (char)0xDBFF + (char)0xDFFF + (char)0xDBFF + (char)0xDFFF;
      string b = "" + (char)0xDBFF + (char)0xDFFF;

      var ordinal = string.CompareOrdinal(a, b);
      var ordinal2 = CultureInfo.InvariantCulture.CompareInfo.Compare(a, b, CompareOptions.None);
      //      CultureInfo.CurrentCulture.TextInfo.

      foreach (CultureInfo cultureInfo in CultureInfo.GetCultures(CultureTypes.AllCultures)) {
        TestLog.Info(cultureInfo.EnglishName);
        int result = cultureInfo.CompareInfo.Compare(x, y, CompareOptions.None);
        Assert.Greater(result, 0);
        result = cultureInfo.CompareInfo.Compare(x, y, CompareOptions.IgnoreKanaType);
        Assert.Greater(result, 0);
        result = cultureInfo.CompareInfo.Compare(x, y, CompareOptions.Ordinal);
        Assert.Greater(result, 0);
        result = cultureInfo.CompareInfo.Compare(x, y, CompareOptions.OrdinalIgnoreCase);
        Assert.Greater(result, 0);
        result = cultureInfo.CompareInfo.Compare(x, y, CompareOptions.StringSort);
        Assert.Greater(result, 0);
      }

      Func<string, string, int> compare0 = CultureInfo.GetCultureInfo(0x7c04).CompareInfo.Compare;
      Func<string, string, int> compare1 = CultureInfo.CurrentCulture.CompareInfo.Compare;
      Func<string, string, int> compare2 = CultureInfo.InvariantCulture.CompareInfo.Compare;
      Func<string, string, CompareOptions, int> compare3 = CultureInfo.CurrentCulture.CompareInfo.Compare;
      Func<string, string, CompareOptions, int> compare4 = CultureInfo.CurrentCulture.CompareInfo.Compare;
      Func<string, string, int> compare5 = string.Compare;

      int actual0 = compare0(x, y);
      int actual1 = compare1(x, y);
      int actual2 = compare2(x, y);
      int actual3 = compare3(x, y, CompareOptions.None);
      int actual4 = compare4(x, y, CompareOptions.None);
      int actual5 = compare3(x, y, CompareOptions.Ordinal);
      int actual6 = compare4(x, y, CompareOptions.Ordinal);
      int actual7 = compare5(x, y);

      Assert.Greater(actual0, 0);
      Assert.Greater(actual1, 0);
      Assert.Greater(actual2, 0);
      Assert.Greater(actual3, 0);
      Assert.Greater(actual4, 0);
      Assert.Greater(actual5, 0);
      Assert.Greater(actual6, 0);
      Assert.Greater(actual7, 0);
    }

    [Test]
    public void PerformanceTest()
    {
      const int iterationCount = 10000000;
      var a = "ASDFGHJK KQ WE RTYUI ZXCV BNDFGHJTY UI XCV BNDF GHJRTYVBNV BN";
      var b = "ASDFGHJK KQ WE RTYUI ZXCV BNDFGHJTY UI KJHFVB<J BNDFGHJTY UI XCV BNDF GHJRTYVBNV BN";

      Func<string, string, CompareOptions, int> cultureCompare = CultureInfo.CurrentCulture.CompareInfo.Compare;
      Func<string, string, CompareOptions, int> invariantCompare = CultureInfo.CurrentCulture.CompareInfo.Compare;
      Func<string, string, int> ordinalCompare = string.CompareOrdinal;
      TestLog.Info("Testing performance of string comparison...");

      using (new Measurement("string.CompareOrdinal", iterationCount))
        for (int i = 0; i < iterationCount; i++)
          ordinalCompare(a, b);

      using (new Measurement("CurrentCulture Compare default", iterationCount))
        for (int i = 0; i < iterationCount; i++)
          cultureCompare(a, b, CompareOptions.None);

      using (new Measurement("CurrentCulture Compare ordinal", iterationCount))
        for (int i = 0; i < iterationCount; i++)
          cultureCompare(a, b, CompareOptions.Ordinal);

      using (new Measurement("InvariantCulture Compare default", iterationCount))
        for (int i = 0; i < iterationCount; i++)
          invariantCompare(a, b, CompareOptions.None);

      using (new Measurement("InvariantCulture Compare ordinal", iterationCount))
        for (int i = 0; i < iterationCount; i++)
          invariantCompare(a, b, CompareOptions.Ordinal);
    }
  }
}