// Copyright (C) 2008-2021 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
// Created by: Alex Yakunin
// Created:    2008.06.05

using System;
using NUnit.Framework;
using Xtensive.Comparison;
using Xtensive.Tuples.Transform;

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
  }
}