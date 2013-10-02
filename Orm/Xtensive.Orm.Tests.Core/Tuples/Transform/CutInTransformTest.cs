// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Elena Vakhtina
// Created:    2008.06.23

using System;
using NUnit.Framework;
using Xtensive.Comparison;
using Xtensive.Tuples;
using Xtensive.Diagnostics;
using Xtensive.Orm.Tests;
using Xtensive.Tuples.Transform;
using Tuple = Xtensive.Tuples.Tuple;


namespace Xtensive.Orm.Tests.Core.Tuples.Transform
{
  [TestFixture]
  public class CutInTransformTest
  {
    public const int IterationCount = 1000000;
    public static Xtensive.Tuples.Tuple t1 = Xtensive.Tuples.Tuple.Create(3, 4.0, "5");
    public static Xtensive.Tuples.Tuple t2 = Xtensive.Tuples.Tuple.Create(1, "2");
    public const int CutInIndex = 1;
    public const string value = "qwe";
    
    [Test]
    public void BaseTest()
    {
      TestLog.InfoRegion("CutInTransform test");
      TestLog.Info("Originals: {0}, {1}, index {2}", t1, t2, CutInIndex);
      CutInTransform ct = new CutInTransform(false, CutInIndex, t1.Descriptor, t2.Descriptor);
      CutInTransform ctro = new CutInTransform(true, CutInIndex, t1.Descriptor, t2.Descriptor);
      Xtensive.Tuples.Tuple wt1 = ct.Apply(TupleTransformType.TransformedTuple, t1, t2);
      TestLog.Info("Wrapper:   {0}", wt1);
      Xtensive.Tuples.Tuple ct1 = ct.Apply(TupleTransformType.Tuple, t1, t2);
      TestLog.Info("Copy:      {0}", ct1);
      Xtensive.Tuples.Tuple wt2 = ctro.Apply(TupleTransformType.TransformedTuple, t1, t2);
      Xtensive.Tuples.Tuple ct2 = ctro.Apply(TupleTransformType.Tuple, t1, t2);
      Assert.AreEqual(wt1, wt2);
      Assert.AreEqual(wt2, ct1);
      Assert.AreEqual(ct1, ct2);

      TestLog.InfoRegion("CutInTransform<T> test");
      TestLog.Info("Originals: {0}, {1}, index {2}", t1, value, CutInIndex);
      CutInTransform<string> ctt = new CutInTransform<string>(false, CutInIndex, t1.Descriptor);
      CutInTransform<string> cttro = new CutInTransform<string>(true, CutInIndex, t1.Descriptor);
      Xtensive.Tuples.Tuple wtt1 = ctt.Apply(TupleTransformType.TransformedTuple, t1, value);
      TestLog.Info("Wrapper:   {0}", wtt1);
      Xtensive.Tuples.Tuple ctt1 = ctt.Apply(TupleTransformType.Tuple, t1, value);
      TestLog.Info("Copy:      {0}", ctt1);
      Xtensive.Tuples.Tuple wtt2 = cttro.Apply(TupleTransformType.TransformedTuple, t1, value);
      Xtensive.Tuples.Tuple ctt2 = cttro.Apply(TupleTransformType.Tuple, t1, value);
      Assert.AreEqual(wtt1, wtt2);
      Assert.AreEqual(wtt2, ctt1);
      Assert.AreEqual(ctt1, ctt2);
      
   }

    [Test]
    [Explicit]
    [Category("Performance")]
    public void PerformanceTest()
    {
      TestLog.InfoRegion("CutInTransform test");
      CutInTransform mt = new CutInTransform(false, CutInIndex, t1.Descriptor, t1.Descriptor);
      Xtensive.Tuples.Tuple wt1 = mt.Apply(TupleTransformType.TransformedTuple, t1, t1);
      Xtensive.Tuples.Tuple wt2 = mt.Apply(TupleTransformType.TransformedTuple, t1, t1);
      Xtensive.Tuples.Tuple ct1 = mt.Apply(TupleTransformType.Tuple, t1, t1);
      Xtensive.Tuples.Tuple ct2 = mt.Apply(TupleTransformType.Tuple, t1, t1);
      PerformanceTransformTesting(ct1, ct2, wt1, wt2);

      TestLog.InfoRegion("CutInTransform<T> test");
      CutInTransform<string> mtt = new CutInTransform<string>(false, CutInIndex, t1.Descriptor);
      Xtensive.Tuples.Tuple wtt1 = mtt.Apply(TupleTransformType.TransformedTuple, t1, value);
      Xtensive.Tuples.Tuple wtt2 = mtt.Apply(TupleTransformType.TransformedTuple, t1, value);
      Xtensive.Tuples.Tuple ctt1 = mtt.Apply(TupleTransformType.Tuple, t1, value);
      Xtensive.Tuples.Tuple ctt2 = mtt.Apply(TupleTransformType.Tuple, t1, value);
      PerformanceTransformTesting(ctt1, ctt2, wtt1, wtt2);

    }

    public void PerformanceTransformTesting(Xtensive.Tuples.Tuple tuple1, Xtensive.Tuples.Tuple tuple2, Xtensive.Tuples.Tuple tuple3, Xtensive.Tuples.Tuple tuple4)
    {
      int count = IterationCount;

      AdvancedComparerStruct<Xtensive.Tuples.Tuple> comparer = AdvancedComparerStruct<Xtensive.Tuples.Tuple>.Default;
      comparer.Equals(tuple1, tuple2);
      comparer.Equals(tuple1, tuple3);
      comparer.Equals(tuple3, tuple4);

      TestHelper.CollectGarbage();
      using (new Measurement("O&O", MeasurementOptions.Log, count))
        for (int i = 0; i < count; i++)
          comparer.Equals(tuple1, tuple2);

      TestHelper.CollectGarbage();
      using (new Measurement("O&W", MeasurementOptions.Log, count))
        for (int i = 0; i < count; i++)
          comparer.Equals(tuple1, tuple3);

      TestHelper.CollectGarbage();
      using (new Measurement("W&W", MeasurementOptions.Log, count))
        for (int i = 0; i < count; i++)
          comparer.Equals(tuple3, tuple4);

    }
  }
}
