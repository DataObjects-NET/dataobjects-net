// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2008.05.27

using System;
using NUnit.Framework;
using Xtensive.Orm.Logging;
using Xtensive.Reflection;
using Xtensive.Diagnostics;
using Xtensive.Orm.Tests;

namespace Xtensive.Orm.Tests.Core.DotNetFramework
{
  [TestFixture]
  public class PointersTest
  {
    public const int IterationCount = 10000000;
    public static bool warmup = true;

    public class Container<T>
    {
      public T Value;

      public Container(T value)
      {
        Value = value;
      }
    }

    [Test]
    public void RegularTest()
    {
      InnerTest(0.001);
    }

    [Test]
    [Explicit]
    [Category("Performance")]
    public void PerformanceTest()
    {
      InnerTest(1);
    }

    private void InnerTest(double speedFactor)
    {
      for (int i = 0; i<2; warmup = (++i)==0) {
        double sf = warmup ? speedFactor / 10 : speedFactor;
        AccessFieldTest<int>(sf);
        AccessFieldTest<double>(sf);
      }
    }

    private void AccessFieldTest<T>(double speedFactor)
    {
      int count = (int)(IterationCount * speedFactor);
      Random r = RandomManager.CreateRandom(SeedVariatorType.CallingMethod);
      T o = InstanceGeneratorProvider.Default.GetInstanceGenerator<T>().GetInstance(r);
      Container<T> c = new Container<T>(o);
      if (!warmup)
        TestLog.Info("Type: {0}, length: {1}", o.GetType().GetShortName(), count);
      using (IndentManager.IncreaseIndent()) {

        if (!warmup)
          TestLog.Info("Direct field access test:");
        using (IndentManager.IncreaseIndent()) {
          TestHelper.CollectGarbage();
          using (new Measurement("Read  ", warmup ? 0 : MeasurementOptions.Log, count))
            for (int i = 0; i<count; i++)
              o = c.Value;
          TestHelper.CollectGarbage();
          using (new Measurement("Write ", warmup ? 0 : MeasurementOptions.Log, count))
            for (int i = 0; i<count; i++)
              c.Value = o;
          TestHelper.CollectGarbage();
        }

        if (!warmup)
          TestLog.Info("TypedReference field access test:");
        using (IndentManager.IncreaseIndent()) {
          var fi = c.GetType().GetField("Value");
          TypedReference tr = __makeref(c);
          TestHelper.CollectGarbage();
          using (new Measurement("GetRef", warmup ? 0 : MeasurementOptions.Log, count))
            for (int i = 0; i<count; i++)
              tr = __makeref(c);
          TestHelper.CollectGarbage();
          using (new Measurement("Read  ", warmup ? 0 : MeasurementOptions.Log, count))
            for (int i = 0; i<count; i++)
              o = (T)fi.GetValueDirect(tr);
          TestHelper.CollectGarbage();
          using (new Measurement("Write ", warmup ? 0 : MeasurementOptions.Log, count))
            for (int i = 0; i<count; i++)
              fi.SetValueDirect(tr, o);
          TestHelper.CollectGarbage();
        }
      }
    }
  }
}