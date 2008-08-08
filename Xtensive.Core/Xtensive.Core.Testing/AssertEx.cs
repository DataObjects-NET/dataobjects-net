// Copyright (C) 2007 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: 
// Created:    2007.10.20

using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using NUnit.Framework;
using Xtensive.Core.Collections;
using Xtensive.Core.Reflection;
using Xtensive.Core.Testing.Resources;

namespace Xtensive.Core.Testing
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

    public static void AreEqual<T>(IEnumerable<T> a, IEnumerable<T> b)
    {
      if (a==null)
        Assert.IsNull(b);
      else {
        SetSlim<T> aSet = new SetSlim<T>(a);
        Assert.IsTrue(aSet.IsEqualTo(b), Strings.AssertCollectionsArentEqual);
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

    public static void Throws<TException>(Action action) 
      where TException: Exception
    {
      try {
        action.Invoke();
        Assert.Fail(String.Format("Expected '{0}' was not thrown.", typeof(TException).GetShortName()));
      } 
      catch (TException) {}
    }

    public static void ThrowsNotSupportedException(Action action)
    {
      Throws<NotSupportedException>(action);
    }

    public static void ThrowsInvalidOperationException(Action action)
    {
      Throws<InvalidOperationException>(action);
    }

    public static void ThrowsArgumentException(Action action)
    {
      Throws<ArgumentException>(action);
    }

    public static void ThrowsArgumentOutOfRangeException(Action action)
    {
      Throws<ArgumentOutOfRangeException>(action);
    }

    public static void ThrowsArgumentNullException(Action action)
    {
      Throws<ArgumentNullException>(action);
    }
  }
}