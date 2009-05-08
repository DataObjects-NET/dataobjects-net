// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexis Kochetov
// Created:    2009.05.08

using System;
using System.Globalization;
using NUnit.Framework;
using Xtensive.Core.Diagnostics;

namespace Xtensive.Core.Tests.Comparison
{
  [TestFixture]
  public class StringComparisonTest
  {
    [Ignore]
    [Test]
    public void StringCompare()
    {
      //      var x = "X" + char.MaxValue + char.MaxValue;
      int z = char.MaxValue;
      int z1 = short.MaxValue;
      int z2 = ushort.MaxValue;
      string x = "X" + (char) 0xDBFF + (char) 0xDFFF;
      string y = "X";

      //      CultureInfo.CurrentCulture.TextInfo.

      foreach (CultureInfo cultureInfo in CultureInfo.GetCultures(CultureTypes.AllCultures)) {
        LogTemplate<Log>.Info(cultureInfo.EnglishName);
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
      int i = 0;
      Assert.Greater(actual0, 0);
      Assert.Greater(actual1, 0);
      Assert.Greater(actual2, 0);
      Assert.Greater(actual3, 0);
      Assert.Greater(actual4, 0);
      Assert.Greater(actual5, 0);
      Assert.Greater(actual6, 0);
      Assert.Greater(actual7, 0);
    }
  }
}