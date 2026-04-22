// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2008.06.05

using System;
using NUnit.Framework;
using Xtensive.Comparison;
using Xtensive.Core;
using Xtensive.Tuples.Transform;

namespace Xtensive.Orm.Tests.Core.Tuples.Transform
{
  [TestFixture]
  public class SegmentTransformTest
  {
    private const int IterationCount = 1000000;
    private const int MeasurementRuns = 5;

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
    public void ComparisonPerformanceTest()
    {
      AdvancedComparerStruct<Xtensive.Tuples.Tuple> comparer = AdvancedComparerStruct<Xtensive.Tuples.Tuple>.Default;
      Xtensive.Tuples.Tuple t   = Xtensive.Tuples.Tuple.Create(1, 2, 3, 4);
      SegmentTransform st = new SegmentTransform(false, t.Descriptor, new Segment<int>(1,2));
      Xtensive.Tuples.Tuple wt1 = st.Apply(TupleTransformType.TransformedTuple, t);
      Xtensive.Tuples.Tuple wt2 = st.Apply(TupleTransformType.TransformedTuple, t);
      Xtensive.Tuples.Tuple ct1 = st.Apply(TupleTransformType.Tuple, t);
      Xtensive.Tuples.Tuple ct2 = st.Apply(TupleTransformType.Tuple, t);
      int count = IterationCount;

      _ = comparer.Equals(ct1, ct2);
      _ = comparer.Equals(ct1, wt1);
      _ = comparer.Equals(wt1, wt2);

      TestHelper.CollectGarbage();
      using (var mx = new Measurement("O&O", MeasurementOptions.Log, count)) {
        for (int i = 0; i<count; i++) {
          _ = comparer.Equals(ct1, ct2);
        }
      }

      TestHelper.CollectGarbage();
      using (var mx = new Measurement("O&W", MeasurementOptions.Log, count)) {
        for (int i = 0; i<count; i++) {
          _ = comparer.Equals(ct1, wt1);
        }
      }

      TestHelper.CollectGarbage();
      using (var mx = new Measurement("W&W", MeasurementOptions.Log, count)) {
        for (int i = 0; i<count; i++) {
          _ = comparer.Equals(wt1, wt2);
        }
      }
    }

    [Test]
    [Explicit]
    [Category("Performance")]
    public void InstanceCreationPerformanceTest()
    {
      Xtensive.Tuples.Tuple t = Xtensive.Tuples.Tuple.Create(1, 2L, "3", "4", 6L, 7);

      int count = IterationCount * 10;

      _ = new SegmentTransform(false, t.Descriptor, new Segment<int>(1, 1));
      _ = new SegmentTransform(false, t.Descriptor, new Segment<int>(1, 2));
      _ = new SegmentTransform(false, t.Descriptor, new Segment<int>(1, 3));
      _ = new SegmentTransform(false, t.Descriptor, new Segment<int>(1, 4));
      _ = new SegmentTransform(false, t.Descriptor, new Segment<int>(1, 5));

      for (var run = 0; run < MeasurementRuns; run++) {
        TestHelper.CollectGarbage();
        using (var mx = new Measurement("S1|1", MeasurementOptions.Log, count)) {
          for (int i = 0; i < count; i++) {
            _ = new SegmentTransform(false, t.Descriptor, new Segment<int>(1, 1));
          }

          mx.Complete();
          Console.WriteLine(mx.ToString());
        }
      }

      Console.WriteLine();
      for (var run = 0; run < MeasurementRuns; run++) {
        TestHelper.CollectGarbage();
        using (var mx = new Measurement("S1|5", MeasurementOptions.Log, count)) {
          for (int i = 0; i < count; i++) {
            _ = new SegmentTransform(false, t.Descriptor, new Segment<int>(1, 5));
          }

          mx.Complete();
          Console.WriteLine(mx.ToString());
        }
      }
    }
  }
}