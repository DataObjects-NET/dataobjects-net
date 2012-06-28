// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2008.04.17

using System;
using System.Collections.Generic;
using NUnit.Framework;
using Xtensive.Diagnostics;
using Xtensive.Testing;

namespace Xtensive.Tests.DotNetFramework
{
  [TestFixture]
  public class CoreFeaturesTest
  {
    public const int IterationCount = 100000000;
    public const int KSize    = 1024;
    public const int MSize    = KSize*1024;
    public const int SizeMin  = 8  * KSize;
    public const int SizeMax  = 16 * MSize;

    [Test]
    [Explicit]
    [Category("Performance")]
    public void PeformanceTest()
    {
      Test(1);
    }

    [Test]
    public void RegularTest()
    {
      Test(0.01);
    }

    private void Test(double speedFactor)
    {
      // Warmup
      TestInt32(100, true);
      TestInt64(100, true);
      TestDouble(100, true);
      TestCast(100, true);
      TestArray(100, true);
      TestList(100, true);
      TestDictionary(100, true);
      // Actual tests
      int count = (int)(IterationCount * speedFactor);
      int sizeMax = TestInfo.IsPerformanceTestRunning ? SizeMax : SizeMax / 8;
      TestInt32(count, false);
      TestInt64(count, false);
      TestDouble(count, false);
      TestCast(count, false);
      for (int size = SizeMin; size<=sizeMax; size *= 2) {
        TestArray(size, false);
        TestList(size, false);
        TestDictionary(size, false);
      }
    }

    private void TestInt32(int count, bool warmup)
    {
      using (warmup ? EmptyDisposable() : Log.InfoRegion("int operations")) {
        int j = 0;
        using (warmup ? EmptyDisposable() : Log.InfoRegion("Unchecked")) unchecked{
          using (warmup ? (IDisposable)new Core.Disposable(delegate { }) : 
            new Measurement("Empty loop", MeasurementOptions.Log, count)) {
            for (int i = 0; i < count; i++) {
            }
          }
          using (warmup ? (IDisposable) new Core.Disposable(delegate { }) :
            new Measurement("Increment int", MeasurementOptions.Log, count)) {
            for (int i = 0; i < count; i+=10) {
              j++;
              j++;
              j++;
              j++;
              j++;
              j++;
              j++;
              j++;
              j++;
              j++;
            }
          }
          using (warmup ? (IDisposable) new Core.Disposable(delegate { }) :
            new Measurement("Adding int", MeasurementOptions.Log, count)) {
            for (int i = 0; i < count; i+=10) {
              j = j + 10;
              j = j + 10;
              j = j + 10;
              j = j + 10;
              j = j + 10;
              j = j + 10;
              j = j + 10;
              j = j + 10;
              j = j + 10;
              j = j + 10;
            }
          }
          using (warmup ? (IDisposable) new Core.Disposable(delegate { }) :
            new Measurement("Multiplying int", MeasurementOptions.Log, count)) {
            for (int i = 0; i < count; i+=10) {
              j = i * 0;
              j = j * 3;
              j = j * 3;
              j = j * 3;
              j = j * 3;
              j = j * 3;
              j = j * 3;
              j = j * 3;
              j = j * 3;
              j = j * 3;
            }
          }
          using (warmup ? (IDisposable) new Core.Disposable(delegate { }) :
            new Measurement("Dividing int", MeasurementOptions.Log, count)) {
            for (int i = 0; i < count; i+=10) {
              j = i / 3;
              j = j / 3;
              j = j / 3;
              j = j / 3;
              j = j / 3;
              j = j / 3;
              j = j / 3;
              j = j / 3;
              j = j / 3;
              j = j / 3;
            }
          }
        }
        using (warmup ? EmptyDisposable() : Log.InfoRegion("Checked")) checked{
          using (warmup ? (IDisposable)new Core.Disposable(delegate { }) : 
            new Measurement("Empty loop", MeasurementOptions.Log, count)) {
            for (int i = 0; i < count; i++) {
            }
          }
          using (warmup ? (IDisposable) new Core.Disposable(delegate { }) :
            new Measurement("Increment int", MeasurementOptions.Log, count)) {
            for (int i = 0; i < count; i+=10) {
              j++;
              j++;
              j++;
              j++;
              j++;
              j++;
              j++;
              j++;
              j++;
              j++;
            }
          }
          using (warmup ? (IDisposable) new Core.Disposable(delegate { }) :
            new Measurement("Adding int", MeasurementOptions.Log, count)) {
            for (int i = 0; i < count; i+=10) {
              j = j + 10;
              j = j + 10;
              j = j + 10;
              j = j + 10;
              j = j + 10;
              j = j + 10;
              j = j + 10;
              j = j + 10;
              j = j + 10;
              j = j + 10;
            }
          }
          using (warmup ? (IDisposable) new Core.Disposable(delegate { }) :
            new Measurement("Multiplying int", MeasurementOptions.Log, count)) {
            for (int i = 0; i < count; i+=10) {
              j = i * 0;
              j = j * 3;
              j = j * 3;
              j = j * 3;
              j = j * 3;
              j = j * 3;
              j = j * 3;
              j = j * 3;
              j = j * 3;
              j = j * 3;
            }
          }
          using (warmup ? (IDisposable) new Core.Disposable(delegate { }) :
            new Measurement("Dividing int", MeasurementOptions.Log, count)) {
            for (int i = 0; i < count; i+=10) {
              j = i / 3;
              j = j / 3;
              j = j / 3;
              j = j / 3;
              j = j / 3;
              j = j / 3;
              j = j / 3;
              j = j / 3;
              j = j / 3;
              j = j / 3;
            }
          }
        }
      }
    }

    private void TestInt64(int count, bool warmup)
    {
      using (warmup ? EmptyDisposable() : Log.InfoRegion("long operations")) {
        long j = 0;
        using (warmup ? EmptyDisposable() : Log.InfoRegion("Unchecked")) unchecked{
          using (warmup ? (IDisposable)new Core.Disposable(delegate { }) : 
            new Measurement("Empty loop", MeasurementOptions.Log, count)) {
            for (long i = 0; i < count; i++) {
            }
          }
          using (warmup ? (IDisposable) new Core.Disposable(delegate { }) :
            new Measurement("Increment long", MeasurementOptions.Log, count)) {
            for (int i = 0; i < count; i+=10) {
              j++;
              j++;
              j++;
              j++;
              j++;
              j++;
              j++;
              j++;
              j++;
              j++;
            }
          }
          using (warmup ? (IDisposable) new Core.Disposable(delegate { }) :
            new Measurement("Adding long", MeasurementOptions.Log, count)) {
            for (long i = 0; i < count; i+=10) {
              j = j + 10;
              j = j + 10;
              j = j + 10;
              j = j + 10;
              j = j + 10;
              j = j + 10;
              j = j + 10;
              j = j + 10;
              j = j + 10;
              j = j + 10;
            }
          }
          using (warmup ? (IDisposable) new Core.Disposable(delegate { }) :
            new Measurement("Multiplying long", MeasurementOptions.Log, count)) {
            for (long i = 0; i < count; i+=10) {
              j = (long)i * 3;
              j = j * 3;
              j = j * 3;
              j = j * 3;
              j = j * 3;
              j = j * 3;
              j = j * 3;
              j = j * 3;
              j = j * 3;
              j = j * 3;
            }
          }
          using (warmup ? (IDisposable) new Core.Disposable(delegate { }) :
            new Measurement("Dividing long", MeasurementOptions.Log, count)) {
            for (long i = 0; i < count; i+=10) {
              j = (long)i / 3;
              j = j / 3;
              j = j / 3;
              j = j / 3;
              j = j / 3;
              j = j / 3;
              j = j / 3;
              j = j / 3;
              j = j / 3;
              j = j / 3;
            }
          }
        }
        using (warmup ? EmptyDisposable() : Log.InfoRegion("Checked")) checked{
          using (warmup ? (IDisposable)new Core.Disposable(delegate { }) : 
            new Measurement("Empty loop", MeasurementOptions.Log, count)) {
            for (long i = 0; i < count; i++) {
            }
          }
          using (warmup ? (IDisposable) new Core.Disposable(delegate { }) :
            new Measurement("Increment long", MeasurementOptions.Log, count)) {
            for (int i = 0; i < count; i+=10) {
              j++;
              j++;
              j++;
              j++;
              j++;
              j++;
              j++;
              j++;
              j++;
              j++;
            }
          }
          using (warmup ? (IDisposable) new Core.Disposable(delegate { }) :
            new Measurement("Adding long", MeasurementOptions.Log, count)) {
            for (long i = 0; i < count; i+=10) {
              j = j + 10;
              j = j + 10;
              j = j + 10;
              j = j + 10;
              j = j + 10;
              j = j + 10;
              j = j + 10;
              j = j + 10;
              j = j + 10;
              j = j + 10;
            }
          }
          using (warmup ? (IDisposable) new Core.Disposable(delegate { }) :
            new Measurement("Multiplying long", MeasurementOptions.Log, count)) {
            for (long i = 0; i < count; i+=10) {
              j = (long)i * 3;
              j = j * 3;
              j = j * 3;
              j = j * 3;
              j = j * 3;
              j = j * 3;
              j = j * 3;
              j = j * 3;
              j = j * 3;
              j = j * 3;
            }
          }
          using (warmup ? (IDisposable) new Core.Disposable(delegate { }) :
            new Measurement("Dividing long", MeasurementOptions.Log, count)) {
            for (long i = 0; i < count; i+=10) {
              j = (long)i / 3;
              j = j / 3;
              j = j / 3;
              j = j / 3;
              j = j / 3;
              j = j / 3;
              j = j / 3;
              j = j / 3;
              j = j / 3;
              j = j / 3;
            }
          }
        }
      }
    }

    private void TestDouble(int count, bool warmup)
    {
      using (warmup ? EmptyDisposable() : Log.InfoRegion("double operations")) {
        double j = 0;
        using (warmup ? EmptyDisposable() : Log.InfoRegion("Unchecked")) unchecked{
          using (warmup ? (IDisposable) new Core.Disposable(delegate { }) :
            new Measurement("Adding double", MeasurementOptions.Log, count)) {
            for (int i = 0; i < count; i+=10) {
              j = j + 10.0;
              j = j + 10.0;
              j = j + 10.0;
              j = j + 10.0;
              j = j + 10.0;
              j = j + 10.0;
              j = j + 10.0;
              j = j + 10.0;
              j = j + 10.0;
              j = j + 10.0;
            }
          }
          using (warmup ? (IDisposable) new Core.Disposable(delegate { }) :
            new Measurement("Multiplying double", MeasurementOptions.Log, count)) {
            for (int i = 0; i < count; i+=10) {
              j = (double)i * 1.0000000000001;
              j = j * 1.0000000000001;
              j = j * 1.0000000000001;
              j = j * 1.0000000000001;
              j = j * 1.0000000000001;
              j = j * 1.0000000000001;
              j = j * 1.0000000000001;
              j = j * 1.0000000000001;
              j = j * 1.0000000000001;
              j = j * 1.0000000000001;
            }
          }
          using (warmup ? (IDisposable) new Core.Disposable(delegate { }) :
            new Measurement("Dividing double", MeasurementOptions.Log, count)) {
            for (int i = 0; i < count; i+=10) {
              j = (double)i / 1.0000000000001;
              j = j / 1.0000000000001;
              j = j / 1.0000000000001;
              j = j / 1.0000000000001;
              j = j / 1.0000000000001;
              j = j / 1.0000000000001;
              j = j / 1.0000000000001;
              j = j / 1.0000000000001;
              j = j / 1.0000000000001;
              j = j / 1.0000000000001;
            }
          }
        }
        using (warmup ? EmptyDisposable() : Log.InfoRegion("Checked")) checked{
          using (warmup ? (IDisposable) new Core.Disposable(delegate { }) :
            new Measurement("Adding double", MeasurementOptions.Log, count)) {
            for (int i = 0; i < count; i+=10) {
              j = j + 10.0;
              j = j + 10.0;
              j = j + 10.0;
              j = j + 10.0;
              j = j + 10.0;
              j = j + 10.0;
              j = j + 10.0;
              j = j + 10.0;
              j = j + 10.0;
              j = j + 10.0;
            }
          }
          using (warmup ? (IDisposable) new Core.Disposable(delegate { }) :
            new Measurement("Multiplying double", MeasurementOptions.Log, count)) {
            for (int i = 0; i < count; i+=10) {
              j = (double)i * 1.0000000000001;
              j = j * 1.0000000000001;
              j = j * 1.0000000000001;
              j = j * 1.0000000000001;
              j = j * 1.0000000000001;
              j = j * 1.0000000000001;
              j = j * 1.0000000000001;
              j = j * 1.0000000000001;
              j = j * 1.0000000000001;
              j = j * 1.0000000000001;
            }
          }
          using (warmup ? (IDisposable) new Core.Disposable(delegate { }) :
            new Measurement("Dividing double", MeasurementOptions.Log, count)) {
            for (int i = 0; i < count; i+=10) {
              j = (double)i / 1.0000000000001;
              j = j / 1.0000000000001;
              j = j / 1.0000000000001;
              j = j / 1.0000000000001;
              j = j / 1.0000000000001;
              j = j / 1.0000000000001;
              j = j / 1.0000000000001;
              j = j / 1.0000000000001;
              j = j / 1.0000000000001;
              j = j / 1.0000000000001;
            }
          }
        }
      }
    }

    private IDisposable EmptyDisposable()
    {
      return new Core.Disposable(delegate { });
    }

    private void TestCast(int size, bool warmup)
    {
      size = size / 10 * 10;
      // Test
      object o = EmptyDisposable();
      object t;
      Core.Disposable d = null;
      IDisposable id = null;
      TestHelper.CollectGarbage();
      using (warmup ? EmptyDisposable() : Log.InfoRegion("Casts")) {
        using (warmup ? (IDisposable) new Core.Disposable(delegate { }) :
          new Measurement("Box int   ", MeasurementOptions.Log, size)) {
          for (int i = 0; i < size; ) {
            t = i++;
            t = i++;
            t = i++;
            t = i++;
            t = i++;
            t = i++;
            t = i++;
            t = i++;
            t = i++;
            t = i++;
          }
        }
        TestHelper.CollectGarbage();
        using (warmup ? (IDisposable) new Core.Disposable(delegate { }) :
          new Measurement("To ancestor ", MeasurementOptions.Log, size)) {
          for (int i = 0; i < size; i+=10) {
            d = (Core.Disposable)o;
            d = (Core.Disposable)o;
            d = (Core.Disposable)o;
            d = (Core.Disposable)o;
            d = (Core.Disposable)o;
            d = (Core.Disposable)o;
            d = (Core.Disposable)o;
            d = (Core.Disposable)o;
            d = (Core.Disposable)o;
            d = (Core.Disposable)o;
          }
        }
        using (warmup ? (IDisposable) new Core.Disposable(delegate { }) :
          new Measurement("To interface", MeasurementOptions.Log, size)) {
          for (int i = 0; i < size; i+=10) {
            id = (IDisposable)o;
            id = (IDisposable)o;
            id = (IDisposable)o;
            id = (IDisposable)o;
            id = (IDisposable)o;
            id = (IDisposable)o;
            id = (IDisposable)o;
            id = (IDisposable)o;
            id = (IDisposable)o;
            id = (IDisposable)o;
          }
        }
        using (warmup ? (IDisposable) new Core.Disposable(delegate { }) :
          new Measurement("To ancestor  (as)", MeasurementOptions.Log, size)) {
          for (int i = 0; i < size; i+=10) {
            d = o as Core.Disposable;
            d = o as Core.Disposable;
            d = o as Core.Disposable;
            d = o as Core.Disposable;
            d = o as Core.Disposable;
            d = o as Core.Disposable;
            d = o as Core.Disposable;
            d = o as Core.Disposable;
            d = o as Core.Disposable;
            d = o as Core.Disposable;
          }
        }
        using (warmup ? (IDisposable) new Core.Disposable(delegate { }) :
          new Measurement("To interface (as)", MeasurementOptions.Log, size)) {
          for (int i = 0; i < size; i+=10) {
            id = o as IDisposable;
            id = o as IDisposable;
            id = o as IDisposable;
            id = o as IDisposable;
            id = o as IDisposable;
            id = o as IDisposable;
            id = o as IDisposable;
            id = o as IDisposable;
            id = o as IDisposable;
            id = o as IDisposable;
          }
        }
      }
    }

    private void TestArray(int size, bool warmup)
    {
      size = size / 10 * 10;
      int[] ints = new int[size];
      IEnumerable<int> eInts = ints;
      for (int i = 0; i < size; i++)
        ints[i] = i;
      // Test
      TestHelper.CollectGarbage();
      int j = size;
      using (warmup ? EmptyDisposable() : Log.InfoRegion(string.Format("int array, {0,6}K", (size+10) / KSize))) {
        using (warmup ? (IDisposable)new Core.Disposable(delegate { }) : 
          new Measurement("Filling (x10)", MeasurementOptions.Log, size)) {
          for (int i = 0; i < size; ) {
            ints[i++] = i;
            ints[i++] = i;
            ints[i++] = i;
            ints[i++] = i;
            ints[i++] = i;
            ints[i++] = i;
            ints[i++] = i;
            ints[i++] = i;
            ints[i++] = i;
            ints[i++] = i;
          }
        }
        using (warmup ? (IDisposable)new Core.Disposable(delegate { }) : 
          new Measurement("Filling (x1) ", MeasurementOptions.Log, size)) {
          for (int i = 0; i < size; i++) {
            ints[i] = i;
          }
        }
        using (warmup ? (IDisposable)new Core.Disposable(delegate { }) : 
          new Measurement("Reading (x10)", MeasurementOptions.Log, size)) {
          for (int i = 0; i < size; ) {
            j = ints[i++];
            j = ints[i++];
            j = ints[i++];
            j = ints[i++];
            j = ints[i++];
            j = ints[i++];
            j = ints[i++];
            j = ints[i++];
            j = ints[i++];
            j = ints[i++];
          }
        }
        using (warmup ? (IDisposable)new Core.Disposable(delegate { }) : 
          new Measurement("Reading (x1) ", MeasurementOptions.Log, size)) {
          for (int i = 0; i < size; ) {
            j = ints[i++];
          }
        }
        using (warmup ? (IDisposable)new Core.Disposable(delegate { }) : 
          new Measurement("Enumerating  ", MeasurementOptions.Log, size)) {
          foreach (int k in eInts) {
          }
        }
      }
      ints = null;
      TestHelper.CollectGarbage();
    }

    private void TestList(int size, bool warmup)
    {
      size = size / 10 * 10;
      List<int> ints = new List<int>(size);
      IEnumerable<int> eInts = ints;
      for (int i = 0; i < size; i++)
        ints.Add(i);
      // Test
      TestHelper.CollectGarbage();
      int j = size;
      using (warmup ? EmptyDisposable() : Log.InfoRegion(string.Format("int list, {0,6}K", (size+10) / KSize))) {
        using (warmup ? (IDisposable)new Core.Disposable(delegate { }) : 
          new Measurement("Filling (x10)", MeasurementOptions.Log, size)) {
          for (int i = 0; i < size; ) {
            ints[i++] = i;
            ints[i++] = i;
            ints[i++] = i;
            ints[i++] = i;
            ints[i++] = i;
            ints[i++] = i;
            ints[i++] = i;
            ints[i++] = i;
            ints[i++] = i;
            ints[i++] = i;
          }
        }
        using (warmup ? (IDisposable)new Core.Disposable(delegate { }) : 
          new Measurement("Filling (x1) ", MeasurementOptions.Log, size)) {
          for (int i = 0; i < size; i++) {
            ints[i] = i;
          }
        }
        using (warmup ? (IDisposable)new Core.Disposable(delegate { }) : 
          new Measurement("Reading (x10)", MeasurementOptions.Log, size)) {
          for (int i = 0; i < size; ) {
            j = ints[i++];
            j = ints[i++];
            j = ints[i++];
            j = ints[i++];
            j = ints[i++];
            j = ints[i++];
            j = ints[i++];
            j = ints[i++];
            j = ints[i++];
            j = ints[i++];
          }
        }
        using (warmup ? (IDisposable)new Core.Disposable(delegate { }) : 
          new Measurement("Reading (x1) ", MeasurementOptions.Log, size)) {
          for (int i = 0; i < size; ) {
            j = ints[i++];
          }
        }
        using (warmup ? (IDisposable)new Core.Disposable(delegate { }) : 
          new Measurement("Enumerating  ", MeasurementOptions.Log, size)) {
          foreach (int k in eInts) {
          }
        }
      }
      ints = null;
      TestHelper.CollectGarbage();
    }

    private void TestDictionary(int size, bool warmup)
    {
      size = size / 10 * 10;
      Dictionary<int, int> ints = new Dictionary<int, int>(size);
      IEnumerable<KeyValuePair<int, int>> eInts = ints;
      // Test
      TestHelper.CollectGarbage();
      int j = size;
      using (warmup ? EmptyDisposable() : Log.InfoRegion(string.Format("int dictionary, {0,6}K", (size+10) / KSize))) {
        ints.Clear();
        using (warmup ? (IDisposable)new Core.Disposable(delegate { }) : 
          new Measurement("Filling (x10)", MeasurementOptions.Log, size)) {
          for (int i = 0; i < size; ) {
            ints[i++] = i;
            ints[i++] = i;
            ints[i++] = i;
            ints[i++] = i;
            ints[i++] = i;
            ints[i++] = i;
            ints[i++] = i;
            ints[i++] = i;
            ints[i++] = i;
            ints[i++] = i;
          }
        }
        ints.Clear();
        using (warmup ? (IDisposable)new Core.Disposable(delegate { }) : 
          new Measurement("Filling (x1) ", MeasurementOptions.Log, size)) {
          for (int i = 0; i < size; i++) {
            ints[i] = i;
          }
        }
        using (warmup ? (IDisposable)new Core.Disposable(delegate { }) : 
          new Measurement("Reading (x10)", MeasurementOptions.Log, size)) {
          for (int i = 0; i < size; ) {
            j = ints[i++];
            j = ints[i++];
            j = ints[i++];
            j = ints[i++];
            j = ints[i++];
            j = ints[i++];
            j = ints[i++];
            j = ints[i++];
            j = ints[i++];
            j = ints[i++];
          }
        }
        using (warmup ? (IDisposable)new Core.Disposable(delegate { }) : 
          new Measurement("Reading (x1) ", MeasurementOptions.Log, size)) {
          for (int i = 0; i < size; ) {
            j = ints[i++];
          }
        }
        using (warmup ? (IDisposable)new Core.Disposable(delegate { }) : 
          new Measurement("Enumerating  ", MeasurementOptions.Log, size)) {
          foreach (KeyValuePair<int, int> k in eInts) {
          }
        }
      }
      ints = null;
      TestHelper.CollectGarbage();
    }
  }
}