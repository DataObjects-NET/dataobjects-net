// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2008.01.24

using System;
using NUnit.Framework;
using Xtensive.Reflection;

namespace Xtensive.Orm.Tests.Core.DotNetFramework
{
  [TestFixture]
  public class GenericFeaturesTest
  {
    private const int IterationCount = 10000000;

    [Test]
    public void EqualityPerformanceTest()
    {
      using(new Measurement("default(int) equals null", IterationCount))
        DefaultEqualToNull<int>();

      using (new Measurement("default(string) equals null", IterationCount))
        DefaultEqualToNull<string>();

      using (new Measurement("default(int?) equals null", IterationCount))
        DefaultEqualToNull<int>();
    }

    private void DefaultEqualToNull<T>()
    {
      for (int i = 0; i < IterationCount; i++) {
        var result = null == default(T);
      }
    }

    [Test]
    public void EqualToNullTest()
    {
      Assert.AreEqual(default(int?), CastTo<int?>(null));
      // int
      EqualToNull<int>(0);
      EqualToNull<int>(1);
      // Nullable<int>
      EqualToNull<int?>(null, "null");
      EqualToNull<int?>(0);
      EqualToNull<int?>(1);
      // String
      EqualToNull<string>(null, "null");
      EqualToNull<string>(string.Empty, "string.Empty");
      EqualToNull<string>("A");
    }

    [Test]
    public void DefaultTest()
    {
      Default<int>();
      Default<int?>();
      Default<string>();
    }

    [Test]
    public void DefaultByReferenceTest()
    {
      DefaultByReference<int>();
      DefaultByReference<int?>();
      DefaultByReference<string>();
    }

    [Test]
    public void IsStructTest()
    {
      InnerIsStruct<int>();
      InnerIsStruct<int?>();
      InnerIsStruct<string>();
    }

    private T CastTo<T>(object o)
    {
      return (T) o;
    }

    private void EqualToNull<T>(T o)
    {
      EqualToNull(o, o is string ? "\""+o+"\"" : o.ToString());
    }

    private void EqualToNull<T>(T o, string oAsString)
    {
      TestLog.Info("{0}: null {1}= {2}.", typeof(T).GetShortName(), null==o ? "=" : "!", oAsString);
    }

    private void Default<T>()
    {
      TestLog.Info("{0}: default {1}= null.", typeof(T).GetShortName(), null==default(T) ? "=" : "!");
    }

    private void DefaultByReference<T>()
    {
      TestLog.Info("{0}: ReferenceEquals(default) {1}= true.", typeof(T).GetShortName(), ReferenceEquals(default(T), null) ? "=" : "!");
    }

    private void InnerIsStruct<T>()
    {
      TestLog.Info("{0}: IsStruct() {1}= true.", typeof(T).GetShortName(), IsStruct<T>() ? "=" : "!");
    }

    private bool IsStruct<T>()
    {
      return typeof (ValueType).IsAssignableFrom(typeof (T));
    }
  }
}