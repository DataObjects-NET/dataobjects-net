// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexis Kochetov
// Created:    2009.09.16

using System;
using System.Diagnostics;
using System.Reflection;
using NUnit.Framework;

namespace Xtensive.Orm.Tests.Core.DotNetFramework
{
  [TestFixture]
  public class StaticMethodTest
  {
    private static Delegate[] delegates;

    class A
    {
      public int x;
    }

    class B : A
    {
      public int y;

      public static int X(A a)
      {
        return a.x;
      }

      public static int Y(A a)
      {
        return ((B)a).y;
      }

      public static int? Z()
      {
        if (typeof(B) == typeof(B)) {
          return 1;
        }
        return null;
      }
    }

    [Test]
    public void Test()
    {
      var b = new B() {x = 1, y = 3};
      var result = B.X(b);
      Console.Out.WriteLine(result);
      result = B.Y(b);
      Console.Out.WriteLine(result);
    }

    private static void Method()
    {
    }

    static StaticMethodTest()
    {
      delegates = new Delegate[2];
      var type = typeof(StaticMethodTest);
      delegates[0] = Delegate.CreateDelegate(typeof(Action), type.GetMethod("Method", BindingFlags.NonPublic | BindingFlags.Static));
      Action func = Method;
      delegates[1] = func;
    }
  }
}