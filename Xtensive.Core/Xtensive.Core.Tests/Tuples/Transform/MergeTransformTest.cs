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
  public class MergeTransformTest
  {
    public const int IterationCount = 1000000;

    [Test]
    public void BaseTest()
    {
      Tuple t1 = Tuple.Create(1, "2");
      Tuple t2 = Tuple.Create(3, 4.0, "5");
      Log.Info("Originals: {0}, {1}", t1, t2);

      CombineTransform mt   = new CombineTransform(false, t1.Descriptor, t2.Descriptor);
      CombineTransform mtro = new CombineTransform(true,  t1.Descriptor, t2.Descriptor);

      Tuple wt1 = mt.Apply(TupleTransformType.TransformedTuple, t1, t2);
      Log.Info("Wrapper:   {0}", wt1);
      Tuple ct1 = mt.Apply(TupleTransformType.Tuple, t1, t2);
      Log.Info("Copy:      {0}", ct1);
      Tuple wt2 = mt.Apply(TupleTransformType.TransformedTuple, t1, t2);
      Tuple ct2 = mt.Apply(TupleTransformType.Tuple, t1, t2);

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

      Tuple wtro = mtro.Apply(TupleTransformType.TransformedTuple, t1, t2);
      AssertEx.Throws<NotSupportedException>(delegate {
        wtro.SetValue(2, 0);
      });

      CombineTransform mt3 = new CombineTransform(false, t1.Descriptor, t1.Descriptor, t1.Descriptor);
      Tuple wt3 = mt3.Apply(TupleTransformType.TransformedTuple, t1, t1, t1);
      Log.Info("Wrapper:   {0}", wt3);
      Tuple ct3 = mt3.Apply(TupleTransformType.Tuple, t1, t1, t1);
      Log.Info("Copy:      {0}", ct3);
      t1.SetValue(0,0);
      Assert.AreEqual(wt3.GetValue(4), t1.GetValue(0));
      t1.SetValue(0,1);

      CombineTransform mt4 = new CombineTransform(false, t1.Descriptor, t1.Descriptor, t1.Descriptor, t1.Descriptor);
      Tuple wt4 = mt4.Apply(TupleTransformType.TransformedTuple, t1, t1, t1, t1);
      Log.Info("Wrapper:   {0}", wt4);
      Tuple ct4 = mt4.Apply(TupleTransformType.Tuple, t1, t1, t1, t1);
      Log.Info("Copy:      {0}", ct4);
      t1.SetValue(0,0);
      Assert.AreEqual(wt4.GetValue(6), t1.GetValue(0));
      t1.SetValue(0,1);
    }

    [Test]
    [Explicit]
    [Category("Performance")]
    public void PerformanceTest1()
    {
      AdvancedComparerStruct<Tuple> comparer = AdvancedComparerStruct<Tuple>.Default;
      Tuple t   = Tuple.Create(1);
      CombineTransform mt = new CombineTransform(false, t.Descriptor, t.Descriptor);
      Tuple wt1 = mt.Apply(TupleTransformType.TransformedTuple, t, t);
      Tuple wt2 = mt.Apply(TupleTransformType.TransformedTuple, t, t);
      Tuple ct1 = mt.Apply(TupleTransformType.Tuple, t, t);
      Tuple ct2 = mt.Apply(TupleTransformType.Tuple, t, t);
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


    [Test]
    [Explicit]
    [Category("Performance")]
    public void PerformanceTest2()
    {
      AdvancedComparerStruct<Tuple> comparer = AdvancedComparerStruct<Tuple>.Default;
      Tuple t   = Tuple.Create(1);
      CombineTransform mt = new CombineTransform(false, t.Descriptor, t.Descriptor, t.Descriptor, t.Descriptor);
      SegmentTransform st = new SegmentTransform(false, mt.Descriptor, new Segment<int>(1,2));
      Tuple wt1 = st.Apply(TupleTransformType.TransformedTuple, mt.Apply(TupleTransformType.TransformedTuple, t, t, t, t));
      Tuple wt2 = st.Apply(TupleTransformType.TransformedTuple, mt.Apply(TupleTransformType.TransformedTuple, t, t, t, t));
      Tuple ct1 = st.Apply(TupleTransformType.Tuple, mt.Apply(TupleTransformType.Tuple, t, t, t, t));
      Tuple ct2 = st.Apply(TupleTransformType.Tuple, mt.Apply(TupleTransformType.Tuple, t, t, t, t));
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