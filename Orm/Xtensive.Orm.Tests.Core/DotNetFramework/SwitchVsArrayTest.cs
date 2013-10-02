// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexis Kochetov
// Created:    2009.09.15

using System;
using System.Diagnostics;
using NUnit.Framework;
using Xtensive.Diagnostics;

namespace Xtensive.Orm.Tests.Core.DotNetFramework
{
  [TestFixture]
  public class SwitchVsArrayTest
  {
    private static object object0 = new object();
    private static object object1 = new object();
    private static object object2 = new object();
    private static object object3 = new object();
    private static object object4 = new object();
    private static object object5 = new object();
    private static object object6 = new object();
    private static object object7 = new object();
    private static object[] objectArray = new[]{object0, object1, object2, object3, object4, object5, object6, object7};
    private const int Length = 8;
    private const int OperationsCount = 10000000;

    [Test]
    public void Test()
    {
      // warmup
      for (int i = 0; i < OperationsCount; i++) {
        var index = i % Length;
        var result = SwitchAccessor(index);
      }

      for (int i = 0; i < OperationsCount; i++) {
        var index = i % Length;
        var result = ArrayAccessor(index);
      }

      using (new Measurement("Accessing array.", OperationsCount * 8)) {
        for (int i = 0; i < OperationsCount; i++) {
          var result = ArrayAccessor(0);
          result = ArrayAccessor(1);
          result = ArrayAccessor(2);
          result = ArrayAccessor(3);
          result = ArrayAccessor(4);
          result = ArrayAccessor(5);
          result = ArrayAccessor(6);
          result = ArrayAccessor(7);
        }
      }
      using (new Measurement("Accessing fields via switch.", OperationsCount * 8)) {
        for (int i = 0; i < OperationsCount; i++) {
          var result = SwitchAccessor(0);
          result = SwitchAccessor(1);
          result = SwitchAccessor(2);
          result = SwitchAccessor(3);
          result = SwitchAccessor(4);
          result = SwitchAccessor(5);
          result = SwitchAccessor(6);
          result = SwitchAccessor(7);
        }
      }
    }

    public virtual object ArrayAccessor(int index)
    {
      return objectArray[index];
    }

    public virtual object SwitchAccessor(int index)
    {
      switch (index) {
        case 0:
          return object0;
        case 1:
          return object1;
        case 2:
          return object2;
        case 3:
          return object3;
        case 4:
          return object4;
        case 5:
          return object5;
        case 6:
          return object6;
        case 7:
          return object7;
        default:
          throw new ArgumentOutOfRangeException();
      }
    }

  }
}