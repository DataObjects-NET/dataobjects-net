// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Elena Vakhtina
// Created:    2008.06.23

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
  public class CutInTransformTest
  {
    public const int IterationCount = 1000000;
    public static Tuple t1 = Tuple.Create(3, 4.0, "5");
    public static Tuple t2 = Tuple.Create(1, "2");
    public const int CutInIndex = 1;
    public const string value = "qwe";
    
    [Test]
    public void BaseTest()
    {
      Log.InfoRegion("CutInTransform test");
      Log.Info("Originals: {0}, {1}, index {2}", t1, t2, CutInIndex);
      CutInTransform ct = new CutInTransform(false, CutInIndex, t1.Descriptor, t2.Descriptor);
      CutInTransform ctro = new CutInTransform(true, CutInIndex, t1.Descriptor, t2.Descriptor);
      Tuple wt1 = ct.Apply(TupleTransformType.TransformedTuple, t1, t2);
      Log.Info("Wrapper:   {0}", wt1);
      Tuple ct1 = ct.Apply(TupleTransformType.Tuple, t1, t2);
      Log.Info("Copy:      {0}", ct1);
      Tuple wt2 = ctro.Apply(TupleTransformType.TransformedTuple, t1, t2);
      Tuple ct2 = ctro.Apply(TupleTransformType.Tuple, t1, t2);
      Assert.AreEqual(wt1, wt2);
      Assert.AreEqual(wt2, ct1);
      Assert.AreEqual(ct1, ct2);

      Log.InfoRegion("CutInTransform<T> test");
      Log.Info("Originals: {0}, {1}, index {2}", t1, value, CutInIndex);
      CutInTransform<string> ctt = new CutInTransform<string>(false, CutInIndex, t1.Descriptor, value);
      CutInTransform<string> cttro = new CutInTransform<string>(true, CutInIndex, t1.Descriptor, value);
      Tuple wtt1 = ctt.Apply(TupleTransformType.TransformedTuple, t1, value);
      Log.Info("Wrapper:   {0}", wtt1);
      Tuple ctt1 = ctt.Apply(TupleTransformType.Tuple, t1, value);
      Log.Info("Copy:      {0}", ctt1);
      Tuple wtt2 = cttro.Apply(TupleTransformType.TransformedTuple, t1, value);
      Tuple ctt2 = cttro.Apply(TupleTransformType.Tuple, t1, value);
      Assert.AreEqual(wtt1, wtt2);
      Assert.AreEqual(wtt2, ctt1);
      Assert.AreEqual(ctt1, ctt2);
      
   }

    [Test]
    [Explicit]
    [Category("Performance")]
    public void PerformanceTest()
    {
      Log.InfoRegion("CutInTransform test");
      CutInTransform mt = new CutInTransform(false, CutInIndex, t1.Descriptor, t1.Descriptor);
      Tuple wt1 = mt.Apply(TupleTransformType.TransformedTuple, t1, t1);
      Tuple wt2 = mt.Apply(TupleTransformType.TransformedTuple, t1, t1);
      Tuple ct1 = mt.Apply(TupleTransformType.Tuple, t1, t1);
      Tuple ct2 = mt.Apply(TupleTransformType.Tuple, t1, t1);
      PerformanceTransformTesting(ct1, ct2, wt1, wt2);

      Log.InfoRegion("CutInTransform<T> test");
      CutInTransform<string> mtt = new CutInTransform<string>(false, CutInIndex, t1.Descriptor, value);
      Tuple wtt1 = mtt.Apply(TupleTransformType.TransformedTuple, t1, value);
      Tuple wtt2 = mtt.Apply(TupleTransformType.TransformedTuple, t1, value);
      Tuple ctt1 = mtt.Apply(TupleTransformType.Tuple, t1, value);
      Tuple ctt2 = mtt.Apply(TupleTransformType.Tuple, t1, value);
      PerformanceTransformTesting(ctt1, ctt2, wtt1, wtt2);

    }

    public void PerformanceTransformTesting(Tuple tuple1, Tuple tuple2, Tuple tuple3, Tuple tuple4)
    {
      int count = IterationCount;

      AdvancedComparerStruct<Tuple> comparer = AdvancedComparerStruct<Tuple>.Default;
      comparer.Compare(tuple1, tuple2);
      comparer.Compare(tuple1, tuple3);
      comparer.Compare(tuple3, tuple4);

      TestHelper.CollectGarbage();
      using (new Measurement("O&O", MeasurementOptions.Log, count))
        for (int i = 0; i < count; i++)
          comparer.Compare(tuple1, tuple2);

      TestHelper.CollectGarbage();
      using (new Measurement("O&W", MeasurementOptions.Log, count))
        for (int i = 0; i < count; i++)
          comparer.Compare(tuple1, tuple3);

      TestHelper.CollectGarbage();
      using (new Measurement("W&W", MeasurementOptions.Log, count))
        for (int i = 0; i < count; i++)
          comparer.Compare(tuple3, tuple4);

    }
  }
}
