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
      TestLog.Info($"Originals: {t1}, {t2}");

      ConcatTransform mt   = new ConcatTransform(false, t1.Descriptor, t2.Descriptor);
      ConcatTransform mtro = new ConcatTransform(true,  t1.Descriptor, t2.Descriptor);

      Xtensive.Tuples.Tuple wt1 = mt.Apply(TupleTransformType.TransformedTuple, t1, t2);
      TestLog.Info($"Wrapper:   {wt1}");
      Xtensive.Tuples.Tuple ct1 = mt.Apply(TupleTransformType.Tuple, t1, t2);
      TestLog.Info($"Copy:      {ct1}");
      Xtensive.Tuples.Tuple wt2 = mt.Apply(TupleTransformType.TransformedTuple, t1, t2);
      Xtensive.Tuples.Tuple ct2 = mt.Apply(TupleTransformType.Tuple, t1, t2);

      Assert.That(wt2, Is.EqualTo(wt1));
      Assert.That(ct1, Is.EqualTo(wt2));
      Assert.That(ct2, Is.EqualTo(ct1));

      wt1.SetValue(2, 0);
      Assert.That(wt1.GetValue(2), Is.EqualTo(t2.GetValue(0)));
      Assert.That(wt2, Is.EqualTo(wt1));
      Assert.That(ct1, Is.Not.EqualTo(wt2));
      Assert.That(ct2, Is.EqualTo(ct1));

      ct1.SetValue(2, 0);
      Assert.That(ct1.GetValue(2), Is.EqualTo(t2.GetValue(0)));
      Assert.That(wt2, Is.EqualTo(wt1));
      Assert.That(ct1, Is.EqualTo(wt2));
      Assert.That(ct2, Is.Not.EqualTo(ct1));

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
      ConcatTransform mt = new ConcatTransform(false, t.Descriptor, t.Descriptor);
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