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
using Xtensive.Orm.Tests;
using Xtensive.Tuples.Transform;
using Tuple = Xtensive.Tuples.Tuple;

namespace Xtensive.Orm.Tests.Core.Tuples.Transform
{
  [TestFixture]
  public class MergeTransformTest
  {
    public const int IterationCount = 1000000;

    [Test]
    public void BaseTest()
    {
      Xtensive.Tuples.Tuple t1 = Xtensive.Tuples.Tuple.Create(1, "2");
      Xtensive.Tuples.Tuple t2 = Xtensive.Tuples.Tuple.Create(3, 4.0, "5");
      TestLog.Info("Originals: {0}, {1}", t1, t2);

      CombineTransform mt   = new CombineTransform(false, t1.Descriptor, t2.Descriptor);
      CombineTransform mtro = new CombineTransform(true,  t1.Descriptor, t2.Descriptor);

      Xtensive.Tuples.Tuple wt1 = mt.Apply(TupleTransformType.TransformedTuple, t1, t2);
      TestLog.Info("Wrapper:   {0}", wt1);
      Xtensive.Tuples.Tuple ct1 = mt.Apply(TupleTransformType.Tuple, t1, t2);
      TestLog.Info("Copy:      {0}", ct1);
      Xtensive.Tuples.Tuple wt2 = mt.Apply(TupleTransformType.TransformedTuple, t1, t2);
      Xtensive.Tuples.Tuple ct2 = mt.Apply(TupleTransformType.Tuple, t1, t2);

      Assert.AreEqual(wt1, wt2);
      Assert.AreEqual(wt2, ct1);
      Assert.AreEqual(ct1, ct2);

      wt1.SetValue(2, 0);
      Assert.AreEqual(t2.GetValue(0), wt1.GetValue(2));
      Assert.AreEqual(wt1, wt2);
      Assert.AreNotEqual(wt2, ct1);
      Assert.AreEqual(ct1, ct2);

      ct1.SetValue(2, 0);
      Assert.AreEqual(t2.GetValue(0), ct1.GetValue(2));
      Assert.AreEqual(wt1, wt2);
      Assert.AreEqual(wt2, ct1);
      Assert.AreNotEqual(ct1, ct2);

      Xtensive.Tuples.Tuple wtro = mtro.Apply(TupleTransformType.TransformedTuple, t1, t2);
      AssertEx.Throws<NotSupportedException>(delegate {
        wtro.SetValue(2, 0);
      });

      CombineTransform mt3 = new CombineTransform(false, t1.Descriptor, t1.Descriptor, t1.Descriptor);
      Xtensive.Tuples.Tuple wt3 = mt3.Apply(TupleTransformType.TransformedTuple, t1, t1, t1);
      TestLog.Info("Wrapper:   {0}", wt3);
      Xtensive.Tuples.Tuple ct3 = mt3.Apply(TupleTransformType.Tuple, t1, t1, t1);
      TestLog.Info("Copy:      {0}", ct3);
      t1.SetValue(0,0);
      Assert.AreEqual(wt3.GetValue(4), t1.GetValue(0));
      t1.SetValue(0,1);

      CombineTransform mt4 = new CombineTransform(false, t1.Descriptor, t1.Descriptor, t1.Descriptor, t1.Descriptor);
      Xtensive.Tuples.Tuple wt4 = mt4.Apply(TupleTransformType.TransformedTuple, t1, t1, t1, t1);
      TestLog.Info("Wrapper:   {0}", wt4);
      Xtensive.Tuples.Tuple ct4 = mt4.Apply(TupleTransformType.Tuple, t1, t1, t1, t1);
      TestLog.Info("Copy:      {0}", ct4);
      t1.SetValue(0,0);
      Assert.AreEqual(wt4.GetValue(6), t1.GetValue(0));
      t1.SetValue(0,1);
    }

    [Test]
    [Explicit]
    [Category("Performance")]
    public void PerformanceTest1()
    {
      AdvancedComparerStruct<Xtensive.Tuples.Tuple> comparer = AdvancedComparerStruct<Xtensive.Tuples.Tuple>.Default;
      Xtensive.Tuples.Tuple t   = Xtensive.Tuples.Tuple.Create(1);
      CombineTransform mt = new CombineTransform(false, t.Descriptor, t.Descriptor);
      Xtensive.Tuples.Tuple wt1 = mt.Apply(TupleTransformType.TransformedTuple, t, t);
      Xtensive.Tuples.Tuple wt2 = mt.Apply(TupleTransformType.TransformedTuple, t, t);
      Xtensive.Tuples.Tuple ct1 = mt.Apply(TupleTransformType.Tuple, t, t);
      Xtensive.Tuples.Tuple ct2 = mt.Apply(TupleTransformType.Tuple, t, t);
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


    [Test]
    [Explicit]
    [Category("Performance")]
    public void PerformanceTest2()
    {
      AdvancedComparerStruct<Xtensive.Tuples.Tuple> comparer = AdvancedComparerStruct<Xtensive.Tuples.Tuple>.Default;
      Xtensive.Tuples.Tuple t   = Xtensive.Tuples.Tuple.Create(1);
      CombineTransform mt = new CombineTransform(false, t.Descriptor, t.Descriptor, t.Descriptor, t.Descriptor);
      SegmentTransform st = new SegmentTransform(false, mt.Descriptor, new Segment<int>(1,2));
      Xtensive.Tuples.Tuple wt1 = st.Apply(TupleTransformType.TransformedTuple, mt.Apply(TupleTransformType.TransformedTuple, t, t, t, t));
      Xtensive.Tuples.Tuple wt2 = st.Apply(TupleTransformType.TransformedTuple, mt.Apply(TupleTransformType.TransformedTuple, t, t, t, t));
      Xtensive.Tuples.Tuple ct1 = st.Apply(TupleTransformType.Tuple, mt.Apply(TupleTransformType.Tuple, t, t, t, t));
      Xtensive.Tuples.Tuple ct2 = st.Apply(TupleTransformType.Tuple, mt.Apply(TupleTransformType.Tuple, t, t, t, t));
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