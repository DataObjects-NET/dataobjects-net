// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kochetov
// Created:    2008.11.21

using System;
using NUnit.Framework;
using Xtensive.Diagnostics;

namespace Xtensive.Tests.DotNetFramework
{
  [TestFixture]
  public class CastTest
  {
    private interface IInterface
    {
      int InterfaceMethod();
    }

    private class Root
    {
      public int Method()
      {
        return 0;
      }
    }

    private class Derived : Root, IInterface
    {
      public int InterfaceMethod()
      {
        return 1;
      }
    }

    private const int operationsCount = (int)1E6;

    [Test]
    public void Test()
    {
      Console.Out.WriteLine("Warmup...");
      InternalTest(10);
      Console.Out.WriteLine("Testing...");
      InternalTest(operationsCount);
    }

    private void InternalTest(int count)
    {
      object o = new Derived();
      Func<int,int> func = i => i;
      Delegate d = func;

      using(new Measurement("Checking cast to class", count)) {
        for (int i = 0; i < count; i++) {
          var result = o is Root;
        }
      }
      using(new Measurement("Casting to class", count)) {
        for (int i = 0; i < count; i++) {
          var root = o as Root;
        }
      }
      using(new Measurement("Casting to interface", count)) {
        for (int i = 0; i < count; i++) {
          var iInterface = o as IInterface;
        }
      }
      using(new Measurement("Casting delegates", count)) {
        for (int i = 0; i < count; i++) {
          var result = d as Func<int,int>;
        }
      }
      using(new Measurement("Casting to class and null-check", count)) {
        for (int i = 0; i < count; i++) {
          var root = o as Root;
          if (root == null)
            throw new InvalidCastException();
        }
      }
      using(new Measurement("Casting to interface and null-check", count)) {
        for (int i = 0; i < count; i++) {
          var iInterface = o as IInterface;
          if (iInterface == null)
            throw new InvalidCastException();
        }
      }
      using(new Measurement("Casting delegates and null-check", count)) {
        for (int i = 0; i < count; i++) {
          var result = d as Func<int,int>;
          if (result == null)
            throw new InvalidCastException();
        }
      }
      using(new Measurement("Explicit casting to class", count)) {
        for (int i = 0; i < count; i++) {
          var root = (Root)o;
        }
      }
      using(new Measurement("Explicit casting to interface", count)) {
        for (int i = 0; i < count; i++) {
          var iInterface = (IInterface)o;
        }
      }
      using(new Measurement("Explicit casting delegates", count)) {
        for (int i = 0; i < count; i++) {
          var result = (Func<int,int>)d;
        }
      }
    }
  }
}