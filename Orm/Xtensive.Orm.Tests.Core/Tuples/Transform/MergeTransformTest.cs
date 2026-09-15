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
    private const int IterationCount = 1_000_000;
    private const int MeasurementRuns = 5;

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
    public void ToStringTest()
    {
      Xtensive.Tuples.Tuple t1 = Xtensive.Tuples.Tuple.Create(1, "2");
      Xtensive.Tuples.Tuple t2 = Xtensive.Tuples.Tuple.Create(3, 4.0, "5");

      var ct = new ConcatTransform(false, t1.Descriptor, t2.Descriptor);
      Assert.That(ct.ToString(), Is.EqualTo("ConcatTransform(TupleDescriptor(Int32, String) + TupleDescriptor(Int32, Double, String), r/w)"));

    }

    [Test]
    [Explicit]
    [Category("Performance")]
    public void ComparisonPerformanceTest()
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
      using (var mx = new Measurement("O&O", MeasurementOptions.Log, count)) {
        for (int i = 0; i < count; i++)
          comparer.Equals(ct1, ct2);

        mx.Complete();
        Console.WriteLine(mx.ToString());
      }

      TestHelper.CollectGarbage();
      using (var mx =new Measurement("O&W", MeasurementOptions.Log, count)) {
        for (int i = 0; i < count; i++)
          comparer.Equals(ct1, wt1);

        mx.Complete();
        Console.WriteLine(mx.ToString());
      }
      
      TestHelper.CollectGarbage();
      using (var mx = new Measurement("W&W", MeasurementOptions.Log, count)) {
        for (int i = 0; i<count; i++)
          comparer.Equals(wt1, wt2);

        mx.Complete();
        Console.WriteLine(mx.ToString());
      }
    }


    [Test]
    [Explicit]
    [Category("Performance")]
    public void InstanceCreationPerformanceTest()
    {
      Xtensive.Tuples.Tuple t1 = Xtensive.Tuples.Tuple.Create(1);
      Xtensive.Tuples.Tuple t2 = Xtensive.Tuples.Tuple.Create(1, "2");
      Xtensive.Tuples.Tuple t3 = Xtensive.Tuples.Tuple.Create(1, 2L, "3");
      Xtensive.Tuples.Tuple t4 = Xtensive.Tuples.Tuple.Create(1, 2L, "3", "4");
      Xtensive.Tuples.Tuple t5 = Xtensive.Tuples.Tuple.Create(1, 2L, "3", "4", 5);
      Xtensive.Tuples.Tuple t6 = Xtensive.Tuples.Tuple.Create(1, 2L, "3", "4", 5, 6L);
     
      int count = IterationCount * 10;

      _ = new ConcatTransform(false, t1.Descriptor, t1.Descriptor);
      _ = new ConcatTransform(false, t2.Descriptor, t2.Descriptor);
      _ = new ConcatTransform(false, t3.Descriptor, t3.Descriptor);
      _ = new ConcatTransform(false, t4.Descriptor, t4.Descriptor);
      _ = new ConcatTransform(false, t5.Descriptor, t5.Descriptor);

      for (var run = 0; run < MeasurementRuns; run++) {
        TestHelper.CollectGarbage();
        using (var mx = new Measurement("N1Concat", MeasurementOptions.Log, count)) {
          for (int i = 0; i < count; i++) {
            _ = new ConcatTransform(false, t1.Descriptor, t1.Descriptor);
          }

          mx.Complete();
          Console.WriteLine(mx.ToString());
        }
      }

      Console.WriteLine();
      for (var run = 0; run < MeasurementRuns; run++) {
        TestHelper.CollectGarbage();
        using (var mx = new Measurement("N5Concat", MeasurementOptions.Log, count)) {
          for (int i = 0; i < count; i++) {
            _ = new ConcatTransform(false, t5.Descriptor, t5.Descriptor);
          }

          mx.Complete();
          Console.WriteLine(mx.ToString());
        }
      }
    }
  }
}