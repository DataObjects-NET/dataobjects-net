// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: 
// Created:    2007.10.20

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using JetBrains.Annotations;
using NUnit.Framework;
using Xtensive.Collections;
using Xtensive.Core;
using Xtensive.Reflection;

namespace Xtensive.Orm.Tests
{
  /// <summary>
  /// Additional assertion methods.
  /// </summary>
  public static class AssertEx
  {
    public static void IsPatternMatch(string source, string pattern)
    {
      pattern = "^"+Regex.Escape(pattern)+"$";
      pattern = pattern.Replace(@"\*", @".*");
      pattern = pattern.Replace(@"\?", @".");
      Regex r = new Regex(pattern, RegexOptions.Singleline | RegexOptions.CultureInvariant);
      Assert.IsTrue(r.IsMatch(source));
    }

    public static void IsNotPatternMatch(string source, string pattern)
    {
      pattern = "^"+Regex.Escape(pattern)+"$";
      pattern = pattern.Replace(@"\*", @".*");
      pattern = pattern.Replace(@"\?", @".");
      Regex r = new Regex(pattern, RegexOptions.Singleline | RegexOptions.CultureInvariant);
      Assert.IsFalse(r.IsMatch(source));
    }

    public static void IsRegexMatch(string source, string regexPattern)
    {
      Regex r = new Regex(regexPattern, RegexOptions.Singleline | RegexOptions.CultureInvariant);
      Assert.IsTrue(r.IsMatch(source));
    }

    public static void IsNotRegexMatch(string source, string regexPattern)
    {
      Regex r = new Regex(regexPattern, RegexOptions.Singleline | RegexOptions.CultureInvariant);
      Assert.IsFalse(r.IsMatch(source));
    }

    public static void HasSameElements<T>(IEnumerable<T> expected, IEnumerable<T> actual)
    {
      if (expected==null)
        Assert.IsNull(actual);
      else {
        var expectedSet = new Set<T>(expected);
        var hasAll = actual.Aggregate(true, (result, current) => result && expectedSet.Contains(current));
        Assert.IsTrue(hasAll, "Collections do not have same elements");
      }
    }

    public static void ResultsAreEqual<T>(Func<T> f1, Func<T> f2)
    {
      T r1 = default(T);
      T r2 = default(T);
      Exception e1 = null;
      try {
        r1 = f1.Invoke();
      }
      catch (Exception e) {
        e1 = e;
      }
      try {
        r2 = f2.Invoke();
      }
      catch (Exception e2) {
        Assert.IsNotNull(e1);
        Assert.AreEqual(e1.GetType(), e2.GetType());
        return;
      }
      Assert.AreEqual(r1, r2);
    }

    public static void Throws<TException>([InstantHandle] Action action) 
      where TException: Exception
    {
      bool thrown = false;
      try {
        action.Invoke();
      } 
      catch (TException) {
        thrown = true;
      }
      if (!thrown)
        Assert.Fail(string.Format("Expected '{0}' was not thrown.", typeof(TException).GetShortName()));
    }

    public static void ThrowsNotSupportedException([InstantHandle] Action action)
    {
      Throws<NotSupportedException>(action);
    }

    public static void ThrowsInvalidOperationException([InstantHandle] Action action)
    {
      Throws<InvalidOperationException>(action);
    }

    public static void ThrowsArgumentException([InstantHandle] Action action)
    {
      Throws<ArgumentException>(action);
    }

    public static void ThrowsArgumentOutOfRangeException([InstantHandle] Action action)
    {
      Throws<ArgumentOutOfRangeException>(action);
    }

    public static void ThrowsArgumentNullException([InstantHandle] Action action)
    {
      Throws<ArgumentNullException>(action);
    }
  }
}