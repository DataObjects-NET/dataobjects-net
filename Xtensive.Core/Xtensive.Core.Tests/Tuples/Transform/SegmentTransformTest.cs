// Copyright (C) 2003-2010 Xtensive LLC.
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
using Tuple = Xtensive.Core.Tuples.Tuple;
using Xtensive.Core.Tuples.Transform;

namespace Xtensive.Core.Tests.Tuples.Transform
{
  [TestFixture]
  public class SegmentTransformTest
  {
    public const int IterationCount = 1000000;

    [Test]
    public void BaseTest()
    {
      Tuple t  = Tuple.Create(1, "2", 3, 4.0);
      Log.Info("Original: {0}", t);

      SegmentTransform st   = new SegmentTransform(false, t.Descriptor, new Segment<int>(1,2));
      SegmentTransform stro = new SegmentTransform(true,  t.Descriptor, new Segment<int>(1,2));

      Tuple wt1 = st.Apply(TupleTransformType.TransformedTuple, t);
      Log.Info("Wrapper:  {0}", wt1);
      Tuple ct1 = st.Apply(TupleTransformType.Tuple, t);
      Log.Info("Copy:     {0}", ct1);
      Tuple wt2 = st.Apply(TupleTransformType.TransformedTuple, t);
      Tuple ct2 = st.Apply(TupleTransformType.Tuple, t);

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

      Tuple wtro = stro.Apply(TupleTransformType.TransformedTuple, t);
      AssertEx.Throws<NotSupportedException>(delegate {
        wtro.SetValue(1, 1);
      });
    }

    [Test]
    [Explicit]
    [Category("Performance")]
    public void PerformanceTest()
    {
      AdvancedComparerStruct<Tuple> comparer = AdvancedComparerStruct<Tuple>.Default;
      Tuple t   = Tuple.Create(1, 2, 3, 4);
      SegmentTransform st = new SegmentTransform(false, t.Descriptor, new Segment<int>(1,2));
      Tuple wt1 = st.Apply(TupleTransformType.TransformedTuple, t);
      Tuple wt2 = st.Apply(TupleTransformType.TransformedTuple, t);
      Tuple ct1 = st.Apply(TupleTransformType.Tuple, t);
      Tuple ct2 = st.Apply(TupleTransformType.Tuple, t);
      int count = IterationCount;

      comparer.Compare(ct1, ct2);
      comparer.Compare(ct1, wt1);
      comparer.Compare(wt1, wt2);

      TestHelper.CollectGarbage();
      using (new Measurement("O&O", MeasurementOptions.Log, count))
        for (int i = 0; i<count; i++)
          comparer.Compare(ct1, ct2);

      TestHelper.CollectGarbage();
      using (new Measurement("O&W", MeasurementOptions.Log, count))
        for (int i = 0; i<count; i++)
          comparer.Compare(ct1, wt1);
      
      TestHelper.CollectGarbage();
      using (new Measurement("W&W", MeasurementOptions.Log, count))
        for (int i = 0; i<count; i++)
          comparer.Compare(wt1, wt2);
    }
  }
}