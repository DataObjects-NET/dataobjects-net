// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2008.01.24

using System;
using NUnit.Framework;
using Xtensive.Core.Reflection;

namespace Xtensive.Core.Tests.DotNetFramework
{
  [TestFixture]
  public class GenericFeaturesTest
  {
    [Test]
    public void EqualToNullTest()
    {
      // Int32
      EqualToNull<int>(0);
      EqualToNull<int>(1);
      // Nullable<Int32>
      EqualToNull<int?>(null, "null");
      EqualToNull<int?>(0);
      EqualToNull<int?>(1);
      // String
      EqualToNull<string>(null, "null");
      EqualToNull<string>(String.Empty, "String.Empty");
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

    private void EqualToNull<T>(T o)
    {
      EqualToNull(o, o is string ? "\""+o+"\"" : o.ToString());
    }

    private void EqualToNull<T>(T o, string oAsString)
    {
      Log.Info("{0}: null {1}= {2}.", typeof(T).GetShortName(), null==o ? "=" : "!", oAsString);
    }

    private void Default<T>()
    {
      Log.Info("{0}: default {1}= null.", typeof(T).GetShortName(), null==default(T) ? "=" : "!");
    }

    private void DefaultByReference<T>()
    {
      Log.Info("{0}: ReferenceEquals(default) {1}= true.", typeof(T).GetShortName(), ReferenceEquals(default(T), null) ? "=" : "!");
    }

    private void InnerIsStruct<T>()
    {
      Log.Info("{0}: IsStruct() {1}= true.", typeof(T).GetShortName(), IsStruct<T>() ? "=" : "!");
    }

    private bool IsStruct<T>()
    {
      return typeof (ValueType).IsAssignableFrom(typeof (T));
    }
  }
}