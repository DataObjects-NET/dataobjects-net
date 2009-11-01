// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2008.06.05

using System;
using NUnit.Framework;
using Xtensive.Core.Comparison;
using Xtensive.Core.Diagnostics;
using Xtensive.Core.Testing;
using Xtensive.Core.Tuples;
using Xtensive.Core.Tuples.Transform;

namespace Xtensive.Core.Tests.Tuples.Transform
{
  [TestFixture]
  public class ReadOnlyTransformTest
  {
    public const int IterationCount = 1000000;

    [Test]
    public void BaseTest()
    {
      Tuple t  = Tuple.Create(1, "2", 3);
      Log.Info("Original: {0}", t);

      Tuple rt = t.ToReadOnly(TupleTransformType.TransformedTuple);
      Log.Info("Wrapper:  {0}", rt);
      Assert.AreEqual(t, rt);
      t.SetValue(0, 2);
      Assert.AreEqual(t, rt);
      t.SetValue(0, 1);
      AssertEx.Throws<NotSupportedException>(delegate {
        rt.SetValue(0, 2);
      });

      Tuple ct = t.ToReadOnly(TupleTransformType.Tuple);
      Log.Info("Copy:     {0}", ct);
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
      AdvancedComparerStruct<Tuple> comparer = AdvancedComparerStruct<Tuple>.Default;
      Tuple t   = Tuple.Create(1, 2);
      Tuple ct  = t.ToRegular();
      Tuple wt  = t.ToReadOnly(TupleTransformType.TransformedTuple);
      Tuple wtc = t.ToReadOnly(TupleTransformType.TransformedTuple);
      int count = IterationCount;

      comparer.Compare(t, ct);
      comparer.Compare(t, wt);
      comparer.Compare(wt, wtc);

      TestHelper.CollectGarbage();
      using (new Measurement("O&O", MeasurementOptions.Log, count))
        for (int i = 0; i<count; i++)
          comparer.Compare(t, ct);

      TestHelper.CollectGarbage();
      using (new Measurement("O&W", MeasurementOptions.Log, count))
        for (int i = 0; i<count; i++)
          comparer.Compare(t, wt);
      
      TestHelper.CollectGarbage();
      using (new Measurement("W&W", MeasurementOptions.Log, count))
        for (int i = 0; i<count; i++)
          comparer.Compare(wt, wtc);
    }
  }
}