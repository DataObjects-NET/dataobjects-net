// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2007.12.17

using System;
using System.Collections.Generic;
using NUnit.Framework;
using Xtensive.Comparison;
using Xtensive.Orm.Tests;

namespace Xtensive.Orm.Tests.Core.Comparison
{
  [TestFixture]
  public class EqualityComparerProviderTests
  {
    [Test]
    public void Int32ComparerTest()
    {
      int o1 = 1;
      int o2 = 2;
      Func<int, int> hash = delegate(int item) { return item.GetHashCode(); };
      long a = hash(o2);
      Func<long, int> hash1 = delegate(long item) { return item.GetHashCode(); };
      long a1 = hash1(o2);

      AdvancedComparer<int> comparer = AdvancedComparer<int>.Default;
      Assert.That(comparer, Is.Not.Null);
      AssertEx.IsPatternMatch(comparer.Implementation.GetType().Name, "Int32Comparer*");
      Assert.That(comparer.Equals(o2,o1), Is.False);
      Assert.That(comparer.GetHashCode(o1)==comparer.GetHashCode(o2), Is.False);
      Assert.That(comparer.Equals(1,o1), Is.True);
      Assert.That(comparer.Equals(2,o2), Is.True);
    }

    [Test]
    public void StringComparerTest()
    {
      string o1 = "1";
      string o2 = "2";
      AdvancedComparer<string> comparer = AdvancedComparer<string>.Default;
      Assert.That(comparer, Is.Not.Null);
      AssertEx.IsPatternMatch(comparer.Implementation.GetType().Name, "StringComparer*");
      Assert.That(comparer.Equals(o2,o1), Is.False);
      Assert.That(comparer.GetHashCode(o1)==comparer.GetHashCode(o2), Is.False);
      Assert.That(comparer.Equals("1",o1), Is.True);
      Assert.That(comparer.Equals("2",o2), Is.True);
    }

    [Test]
    public void InheritedComparerTest()
    {
      Wrapper2a<int, int> o1 = new Wrapper2a<int, int>(0,1);
      Wrapper2a<int, int> o2 = new Wrapper2a<int, int>(0,2);
      AdvancedComparer<Wrapper2a<int, int>> comparer = AdvancedComparer<Wrapper2a<int, int>>.Default;
      Assert.That(comparer, Is.Not.Null);
      AssertEx.IsPatternMatch(comparer.Implementation.GetType().Name, "BaseComparerWrapper*");
      Assert.That(comparer.Equals(o2,o1), Is.True);
      Assert.That(comparer.GetHashCode(o1)==comparer.GetHashCode(o2), Is.True);
    }

    [Test]
    public void GetCashCodeInvocationTest()
    {
      int i = 5;
      object o = i;
      Assert.That(GetHashCodeInvocationOnObject(o), Is.EqualTo(GetHashCodeInvocationOnStruct(i)));
    }


    private static int GetHashCodeInvocationOnStruct(int i)
    {
      return i.GetHashCode();
    }

    private static int GetHashCodeInvocationOnObject(object o)
    {
      return o.GetHashCode();
    }
  }
}