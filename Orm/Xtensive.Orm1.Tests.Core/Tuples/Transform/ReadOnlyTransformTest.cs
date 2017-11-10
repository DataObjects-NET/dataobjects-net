// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2008.06.05

using System;
using NUnit.Framework;
using Xtensive.Comparison;
using Xtensive.Tuples;
using Xtensive.Orm.Tests;
using Xtensive.Tuples.Transform;
using Tuple = Xtensive.Tuples.Tuple;

namespace Xtensive.Orm.Tests.Core.Tuples.Transform
{
  [TestFixture]
  public class ReadOnlyTransformTest
  {
    public const int IterationCount = 1000000;

    [Test]
    public void BaseTest()
    {
      Xtensive.Tuples.Tuple t  = Xtensive.Tuples.Tuple.Create(1, "2", 3);
      TestLog.Info("Original: {0}", t);

      Xtensive.Tuples.Tuple rt = t.ToReadOnly(TupleTransformType.TransformedTuple);
      TestLog.Info("Wrapper:  {0}", rt);
      Assert.AreEqual(t, rt);
      t.SetValue(0, 2);
      Assert.AreEqual(t, rt);
      t.SetValue(0, 1);
      AssertEx.Throws<NotSupportedException>(delegate {
        rt.SetValue(0, 2);
      });

      Xtensive.Tuples.Tuple ct = t.ToReadOnly(TupleTransformType.Tuple);
      TestLog.Info("Copy:     {0}", ct);
      Assert.AreEqual(t, ct);
      t.SetValue(0, 2);
      Assert.AreNotEqual(t, ct);
      t.SetValue(0, 1);
      AssertEx.Throws<NotSupportedException>(delegate {
        ct.SetValue(0, 2);
      });
    }

    [Test]
    [Explicit]
    [Category("Performance")]
    public void PerformanceTest()
    {
      AdvancedComparerStruct<Xtensive.Tuples.Tuple> comparer = AdvancedComparerStruct<Xtensive.Tuples.Tuple>.Default;
      Xtensive.Tuples.Tuple t   = Xtensive.Tuples.Tuple.Create(1, 2);
      Xtensive.Tuples.Tuple ct  = t.ToRegular();
      Xtensive.Tuples.Tuple wt  = t.ToReadOnly(TupleTransformType.TransformedTuple);
      Xtensive.Tuples.Tuple wtc = t.ToReadOnly(TupleTransformType.TransformedTuple);
      int count = IterationCount;

      comparer.Equals(t, ct);
      comparer.Equals(t, wt);
      comparer.Equals(wt, wtc);

      TestHelper.CollectGarbage();
      using (new Measurement("O&O", MeasurementOptions.Log, count))
        for (int i = 0; i<count; i++)
          comparer.Equals(t, ct);

      TestHelper.CollectGarbage();
      using (new Measurement("O&W", MeasurementOptions.Log, count))
        for (int i = 0; i<count; i++)
          comparer.Equals(t, wt);
      
      TestHelper.CollectGarbage();
      using (new Measurement("W&W", MeasurementOptions.Log, count))
        for (int i = 0; i<count; i++)
          comparer.Equals(wt, wtc);
    }
  }
}