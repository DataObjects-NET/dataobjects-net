// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2008.06.05

using System;
using NUnit.Framework;
using Xtensive.Comparison;
using Xtensive.Core;
using Xtensive.Tuples;
using Xtensive.Diagnostics;
using Xtensive.Testing;
using Xtensive.Tuples.Transform;
using Tuple = Xtensive.Tuples.Tuple;

namespace Xtensive.Tests.Tuples.Transform
{
  [TestFixture]
  public class SegmentTransformTest
  {
    public const int IterationCount = 1000000;

    [Test]
    public void BaseTest()
    {
      Xtensive.Tuples.Tuple t  = Xtensive.Tuples.Tuple.Create(1, "2", 3, 4.0);
      Log.Info("Original: {0}", t);

      SegmentTransform st   = new SegmentTransform(false, t.Descriptor, new Segment<int>(1,2));
      SegmentTransform stro = new SegmentTransform(true,  t.Descriptor, new Segment<int>(1,2));

      Xtensive.Tuples.Tuple wt1 = st.Apply(TupleTransformType.TransformedTuple, t);
      Log.Info("Wrapper:  {0}", wt1);
      Xtensive.Tuples.Tuple ct1 = st.Apply(TupleTransformType.Tuple, t);
      Log.Info("Copy:     {0}", ct1);
      Xtensive.Tuples.Tuple wt2 = st.Apply(TupleTransformType.TransformedTuple, t);
      Xtensive.Tuples.Tuple ct2 = st.Apply(TupleTransformType.Tuple, t);

      Assert.AreEqual(wt1, wt2);
      Assert.AreEqual(wt2, ct1);
      Assert.AreEqual(ct1, ct2);

      wt1.SetValue(1, 1);
      Assert.AreEqual(t.GetValue(2), wt1.GetValue(1));
      Assert.AreEqual(wt1, wt2);
      Assert.AreNotEqual(wt2, ct1);
      Assert.AreEqual(ct1, ct2);

      ct1.SetValue(1, 1);
      Assert.AreEqual(t.GetValue(2), ct1.GetValue(1));
      Assert.AreEqual(wt1, wt2);
      Assert.AreEqual(wt2, ct1);
      Assert.AreNotEqual(ct1, ct2);

      Xtensive.Tuples.Tuple wtro = stro.Apply(TupleTransformType.TransformedTuple, t);
      AssertEx.Throws<NotSupportedException>(delegate {
        wtro.SetValue(1, 1);
      });
    }

    [Test]
    [Explicit]
    [Category("Performance")]
    public void PerformanceTest()
    {
      AdvancedComparerStruct<Xtensive.Tuples.Tuple> comparer = AdvancedComparerStruct<Xtensive.Tuples.Tuple>.Default;
      Xtensive.Tuples.Tuple t   = Xtensive.Tuples.Tuple.Create(1, 2, 3, 4);
      SegmentTransform st = new SegmentTransform(false, t.Descriptor, new Segment<int>(1,2));
      Xtensive.Tuples.Tuple wt1 = st.Apply(TupleTransformType.TransformedTuple, t);
      Xtensive.Tuples.Tuple wt2 = st.Apply(TupleTransformType.TransformedTuple, t);
      Xtensive.Tuples.Tuple ct1 = st.Apply(TupleTransformType.Tuple, t);
      Xtensive.Tuples.Tuple ct2 = st.Apply(TupleTransformType.Tuple, t);
      int count = IterationCount;

      comparer.Equals(ct1, ct2);
      comparer.Equals(ct1, wt1);
      comparer.Equals(wt1, wt2);

      TestHelper.CollectGarbage();
      using (new Measurement("O&O", MeasurementOptions.Log, count))
        for (int i = 0; i<count; i++)
          comparer.Equals(ct1, ct2);

      TestHelper.CollectGarbage();
      using (new Measurement("O&W", MeasurementOptions.Log, count))
        for (int i = 0; i<count; i++)
          comparer.Equals(ct1, wt1);
      
      TestHelper.CollectGarbage();
      using (new Measurement("W&W", MeasurementOptions.Log, count))
        for (int i = 0; i<count; i++)
          comparer.Equals(wt1, wt2);
    }
  }
}