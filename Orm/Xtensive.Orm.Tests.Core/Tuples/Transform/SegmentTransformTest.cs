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
using Xtensive.Orm.Tests;
using Xtensive.Tuples.Transform;
using Tuple = Xtensive.Tuples.Tuple;

namespace Xtensive.Orm.Tests.Core.Tuples.Transform
{
  [TestFixture]
  public class SegmentTransformTest
  {
    public const int IterationCount = 1000000;

    [Test]
    public void BaseTest()
    {
      Xtensive.Tuples.Tuple t  = Xtensive.Tuples.Tuple.Create(1, "2", 3, 4.0);
      TestLog.Info($"Original: {t}");

      SegmentTransform st   = new SegmentTransform(false, t.Descriptor, new Segment<int>(1,2));
      SegmentTransform stro = new SegmentTransform(true,  t.Descriptor, new Segment<int>(1,2));

      Xtensive.Tuples.Tuple wt1 = st.Apply(TupleTransformType.TransformedTuple, t);
      TestLog.Info($"Wrapper:  {wt1}");
      Xtensive.Tuples.Tuple ct1 = st.Apply(TupleTransformType.Tuple, t);
      TestLog.Info($"Copy:     {ct1}");
      Xtensive.Tuples.Tuple wt2 = st.Apply(TupleTransformType.TransformedTuple, t);
      Xtensive.Tuples.Tuple ct2 = st.Apply(TupleTransformType.Tuple, t);

      Assert.That(wt2, Is.EqualTo(wt1));
      Assert.That(ct1, Is.EqualTo(wt2));
      Assert.That(ct2, Is.EqualTo(ct1));

      wt1.SetValue(1, 1);
      Assert.That(wt1.GetValue(1), Is.EqualTo(t.GetValue(2)));
      Assert.That(wt2, Is.EqualTo(wt1));
      Assert.That(ct1, Is.Not.EqualTo(wt2));
      Assert.That(ct2, Is.EqualTo(ct1));

      ct1.SetValue(1, 1);
      Assert.That(ct1.GetValue(1), Is.EqualTo(t.GetValue(2)));
      Assert.That(wt2, Is.EqualTo(wt1));
      Assert.That(ct1, Is.EqualTo(wt2));
      Assert.That(ct2, Is.Not.EqualTo(ct1));

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