// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2008.01.20

using System;
using System.Collections.Generic;
using NUnit.Framework;
using Xtensive.Core.Collections;
using Xtensive.Core.Diagnostics;

namespace Xtensive.Core.Tests.Collections
{
  [TestFixture]
  public class TypeBasedDictionaryTest
  {
    private Dictionary<Type, int> rd  = new Dictionary<Type, int>();
    private TypeBasedDictionary   tbd = TypeBasedDictionary.Create();
    private Type tbool = typeof (bool);
    private Type tchar = typeof (char);
    private Type tbyte = typeof (byte);
    private Type tshort = typeof (short);
    private Type tushort = typeof (ushort);
    private Type tint = typeof (int);
    private Type tuint = typeof (uint);
    private Type tlong = typeof (long);
    private Type tulong = typeof (ulong);
    private Type tstring = typeof (string);

    [Test]
    public void CombinedTest()
    {
      TypeBasedDictionary d1 = TypeBasedDictionary.Create();
      Assert.AreEqual(d1.GetValue<int, int>(), 0);
      Assert.AreEqual(d1.GetValue<int, bool>(), false);
      d1.SetValue<int, int>(1);
      d1.SetValue<int, bool>(true);
      Assert.AreEqual(d1.GetValue<int, int>(), 1);
      Assert.AreEqual(d1.GetValue<int, bool>(), true);

      TypeBasedDictionary d2 = TypeBasedDictionary.Create();
      Assert.AreEqual(d2.GetValue<int, int>(), 0);
      d2.SetValue<int, int>(2);
      Assert.AreEqual(d2.GetValue<int, int>(), 2);

      Assert.AreNotEqual(d1.GetValue<int, int>(), d2.GetValue<int, int>());
    }

    [Test]
    [Explicit]
    [Category("Performance")]
    public void PerformanceTest()
    {
      FillTypeBasedDictionary();
      FillDictionary();
      DictionaryReadTest(1000);
      TypeBasedDictionaryReadTest(1000);
      int count = 1000000;
      int r1, r2;
      using (new Measurement("Reading TypeBasedDictionary", count*10)) {
        r1 = TypeBasedDictionaryReadTest(count);
      }
      using (new Measurement("Reading Dictionary", count*10)) {
        r2 = DictionaryReadTest(count);
      }
      Assert.AreEqual(r1, r2);
    }

    private void FillTypeBasedDictionary()
    {
      tbd.SetValue<bool, int>(1);
      tbd.SetValue<char, int>(2);
      tbd.SetValue<byte, int>(3);
      tbd.SetValue<short, int>(4);
      tbd.SetValue<ushort, int>(5);
      tbd.SetValue<int, int>(6);
      tbd.SetValue<uint, int>(7);
      tbd.SetValue<long, int>(8);
      tbd.SetValue<ulong, int>(9);
      tbd.SetValue<string, int>(10);
    }

    private void FillDictionary()
    {
      rd.Add(tbool, 1);
      rd.Add(tchar, 2);
      rd.Add(tbyte, 3);
      rd.Add(tshort, 4);
      rd.Add(tushort, 5);
      rd.Add(tint, 6);
      rd.Add(tuint, 7);
      rd.Add(tlong, 8);
      rd.Add(tulong, 9);
      rd.Add(tstring, 10);
    }

    private int TypeBasedDictionaryReadTest(int count)
    {
      int j = 0;
      unchecked {
        for (int i = 0; i < count; i++) {
          j += tbd.GetValue<bool, int>();
          j += tbd.GetValue<char, int>();
          j += tbd.GetValue<byte, int>();
          j += tbd.GetValue<short, int>();
          j += tbd.GetValue<ushort, int>();
          j += tbd.GetValue<int, int>();
          j += tbd.GetValue<uint, int>();
          j += tbd.GetValue<long, int>();
          j += tbd.GetValue<ulong, int>();
          j += tbd.GetValue<string, int>();
        }
      }
      return j;
    }

    private int DictionaryReadTest(int count)
    {
      int j = 0;
      unchecked {
        for (int i = 0; i < count; i++) {
          j += rd[tbool];
          j += rd[tchar];
          j += rd[tbyte];
          j += rd[tshort];
          j += rd[tushort];
          j += rd[tint];
          j += rd[tuint];
          j += rd[tlong];
          j += rd[tulong];
          j += rd[tstring];
        }
      }
      return j;
    }
  }
}