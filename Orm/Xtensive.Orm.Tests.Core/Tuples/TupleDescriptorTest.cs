// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2008.01.25

using System;
using System.Collections.Generic;
using NUnit.Framework;
using Xtensive.Tuples;

namespace Xtensive.Orm.Tests.Core.Tuples
{
  [TestFixture]
  public class TupleDescriptorTest
  {
    public readonly Type[] FieldTypes = new Type[] {
      typeof (bool),
      typeof (bool?),
      typeof (byte),
      typeof (byte?),
      typeof (sbyte),
      typeof (sbyte?),
      typeof (char),
      typeof (char?),
      typeof (short),
      typeof (short?),
      typeof (int),
      typeof (int?),
      typeof (long),
      typeof (long?),
      typeof (float),
      typeof (float?),
      typeof (double),
      typeof (double?),
      typeof (KeyValuePair<int, int>),
      typeof (KeyValuePair<int?, int>),
      typeof (KeyValuePair<int, int?>),
      typeof (KeyValuePair<int?, int?>),
      typeof (KeyValuePair<int, int>?),
      typeof (KeyValuePair<int?, int>?),
      typeof (KeyValuePair<int, int?>?),
      typeof (KeyValuePair<int?, int?>?),
      typeof (string)
    };

    [Test]
    [Explicit]
    [Category("Performance")]
    public void PerformanceTest()
    {
      var rnd = RandomManager.CreateRandom(SeedVariatorType.CallingMethod);
      var count = 100;
      var types = new List<Type>();
      var descriptors = new List<TupleDescriptor>();
      using (new Measurement("Creating descriptors {T1}, {T1,T2}, ...", count)) {
        for (var i = 0; i<count; i++) {
          types.Add(typeof (bool));
          descriptors.Add(TupleDescriptor.Create(types.ToArray()));
        }
      }

      count = 1000;
      var maxSize = 10;
      int size;
      using (new Measurement("Creating random descriptors", count)) {
        for (var i = 0; i<count; i++) {
          size = rnd.Next(maxSize);
          types = new List<Type>(size);
          for (var j = 0; j < size; j++)
            types.Add(FieldTypes[rnd.Next(FieldTypes.Length)]);
          descriptors.Add(TupleDescriptor.Create(types.ToArray()));
        }
      }

      count = 100000;
      size = 10;
      types = new List<Type>(size);
      for (var j = 0; j < size; j++)
        types.Add(FieldTypes[rnd.Next(FieldTypes.Length)]);
      using (new Measurement("Creating the same descriptor", count)) {
        for (var i = 0; i<count; i++)
          descriptors.Add(TupleDescriptor.Create(types.ToArray()));
      }
    }

    [Test]
    [Explicit]
    [Category("Performance")]
    public void PerformanceOfConcatTest()
    {
      var rnd = RandomManager.CreateRandom(SeedVariatorType.CallingMethod);
      var count = 100000;
      var types = new List<Type>();

      var size = 1;
      var sizeBig = 25;
      for (var i = 0; i < size; i++)
        types.Add(FieldTypes[rnd.Next(FieldTypes.Length)]);
      var firstTiny = TupleDescriptor.Create(types.ToArray());
      var secondTiny = TupleDescriptor.Create(types.ToArray());

      types = new List<Type>(25);
      for (var i = 0; i < sizeBig; i++)
        types.Add(FieldTypes[rnd.Next(FieldTypes.Length)]);
      var firstBig = TupleDescriptor.Create(types.ToArray());
      var secondBig = TupleDescriptor.Create(types.ToArray());

      _ = firstTiny.ConcatWith(secondTiny);
      _ = firstBig.ConcatWith(secondBig);

      for (var runIdx = 0; runIdx < 10; runIdx++) {
        TestHelper.CollectGarbage();

        using (var mx = new Measurement("Concating descriptors", count)) {
          for (var i = 0; i < count; i++) {
            _ = firstTiny.ConcatWith(secondTiny);
          }
          mx.Complete();
          Console.WriteLine(mx.ToString());
        }
      }

      Console.WriteLine();
      for (var runIdx = 0; runIdx < 10; runIdx++) {
        TestHelper.CollectGarbage();

        using (var mx = new Measurement("Concating descriptors", count)) {
          for (var i = 0; i < count; i++) {
            _ = firstBig.ConcatWith(secondBig);
          }
          mx.Complete();
          Console.WriteLine(mx.ToString());
        }
      }
    }

    [Test]
    public void GetHeadingPartTest()
    {
      var dAll = TupleDescriptor.Create(FieldTypes);
      _ = Assert.Throws<ArgumentOutOfRangeException>(() => dAll.Head(-1));
      var head1 = dAll.Head(1);
      Assert.That(head1.Count, Is.EqualTo(1));
      Assert.That(head1[0], Is.EqualTo(dAll[0]));
      var head5 = dAll.Head(5);
      Assert.That(head5.Count, Is.EqualTo(5));
      Assert.That(head5[0], Is.EqualTo(dAll[0]));
      Assert.That(head5[1], Is.EqualTo(dAll[1]));
      Assert.That(head5[2], Is.EqualTo(dAll[2]));
      Assert.That(head5[3], Is.EqualTo(dAll[3]));
      Assert.That(head5[4], Is.EqualTo(dAll[4]));

      _ = Assert.Throws<ArgumentOutOfRangeException>(() => dAll.Head(FieldTypes.Length + 1));
    }

    [Test]
    public void GetTailPartTest()
    {
      var dAll = TupleDescriptor.Create(FieldTypes);
      _ = Assert.Throws<ArgumentOutOfRangeException>(() => dAll.Tail(0));
      var tail1 = dAll.Tail(1);
      Assert.That(tail1.Count, Is.EqualTo(1));
      Assert.That(tail1[0], Is.EqualTo(dAll[^1]));
      var tail5 = dAll.Tail(5);
      Assert.That(tail5.Count, Is.EqualTo(5));
      Assert.That(tail5[0], Is.EqualTo(dAll[^5]));
      Assert.That(tail5[1], Is.EqualTo(dAll[^4]));
      Assert.That(tail5[2], Is.EqualTo(dAll[^3]));
      Assert.That(tail5[3], Is.EqualTo(dAll[^2]));
      Assert.That(tail5[4], Is.EqualTo(dAll[^1]));

      _ = Assert.Throws<ArgumentOutOfRangeException>(() => dAll.Tail(FieldTypes.Length + 1));
    }

    [Test]
    public void GetSegmentTest()
    {
      var dAll = TupleDescriptor.Create(FieldTypes);
      _ = Assert.Throws<ArgumentOutOfRangeException>(() => dAll.Segment(new Xtensive.Core.Segment<int>(-1, 2)));

      var head2 = dAll.Segment(new Xtensive.Core.Segment<int>(0, 2));
      Assert.That(head2.Count, Is.EqualTo(2));
      Assert.That(head2[0], Is.EqualTo(dAll[0]));
      Assert.That(head2[1], Is.EqualTo(dAll[1]));
      var middle5 = dAll.Segment(new Xtensive.Core.Segment<int>(5, 5));
      Assert.That(middle5.Count, Is.EqualTo(5));
      Assert.That(middle5[0], Is.EqualTo(dAll[5]));
      Assert.That(middle5[1], Is.EqualTo(dAll[6]));
      Assert.That(middle5[2], Is.EqualTo(dAll[7]));
      Assert.That(middle5[3], Is.EqualTo(dAll[8]));
      Assert.That(middle5[4], Is.EqualTo(dAll[9]));
      var complete = dAll.Segment(new Xtensive.Core.Segment<int>(0, dAll.Count));
      Assert.That(complete, Is.EqualTo(dAll));

      _ = Assert.Throws<ArgumentException>(() => dAll.Segment(new Xtensive.Core.Segment<int>(0, FieldTypes.Length + 2)));
      _ = Assert.Throws<ArgumentException>(() => dAll.Segment(new Xtensive.Core.Segment<int>(1, FieldTypes.Length + 3)));
      _ = Assert.Throws<ArgumentException>(() => dAll.Segment(new Xtensive.Core.Segment<int>(2, FieldTypes.Length + 4)));
    }

    [Test]
    public void ConcatDescriptorsTest()
    {
      var d1 = TupleDescriptor.Create(new Type[] { typeof(bool), typeof(int?), typeof(string) });
      var d2 = TupleDescriptor.Create(new Type[] { typeof(bool?), typeof(int?) });
      Assert.That(d1, Is.Not.EqualTo(default(TupleDescriptor)));
      Assert.That(d2, Is.Not.EqualTo(default(TupleDescriptor)));
      
      var concated = d1.ConcatWith(d2);
      Assert.That(concated, Is.Not.EqualTo(default(TupleDescriptor)));
      Assert.That(concated.Count, Is.EqualTo(d1.Count + d2.Count));

      for (int firstPart = 0; firstPart < d1.Count; firstPart++) {
        Assert.That(concated[firstPart], Is.EqualTo(d1[firstPart]));
      }

      for (int secondPart = d1.Count, origIndex = 0; secondPart < d2.Count; secondPart++, origIndex++) {
        Assert.That(concated[secondPart], Is.EqualTo(d2[origIndex]));
      }
    }

    [Test]
    public void CombinedTest()
    {
      TupleDescriptor desc;

      desc = TestDescriptor(TupleDescriptor.Empty, new Type[] {});
      desc = TestDescriptor(desc, new Type[] {});

      desc = TestDescriptor(null, new Type[] {typeof(bool)});
      desc = TestDescriptor(desc, new Type[] {typeof(bool?)});

      desc = TestDescriptor(null, new Type[] {typeof(int)});
      desc = TestDescriptor(desc, new Type[] {typeof(int?)});

      desc = TestDescriptor(null, new Type[] {typeof(string)});
      desc = TestDescriptor(desc, new Type[] {typeof(string)});
      
      desc = TestDescriptor(null, new Type[] {typeof(bool), typeof(bool)});
      desc = TestDescriptor(desc, new Type[] {typeof(bool?), typeof(bool?)});

      desc = TestDescriptor(null, new Type[] {typeof(bool), typeof(int)});
      desc = TestDescriptor(desc, new Type[] {typeof(bool?), typeof(int?)});

      desc = TestDescriptor(null, new Type[] {typeof(bool), typeof(string)});
      desc = TestDescriptor(desc, new Type[] {typeof(bool?), typeof(string)});

      desc = TestDescriptor(null, new Type[] {typeof(bool), typeof(int), typeof(string)});
      desc = TestDescriptor(desc, new Type[] {typeof(bool), typeof(int?), typeof(string)});

      desc = TestDescriptor(null, new Type[] {typeof(bool), typeof(bool), typeof(bool)});
      desc = TestDescriptor(desc, new Type[] {typeof(bool?), typeof(bool), typeof(bool?)});
    }

    private TupleDescriptor TestDescriptor(TupleDescriptor? theSame, Type[] types)
    {
      var d1 = TupleDescriptor.Create(types);
      var d2 = TupleDescriptor.Create(types);
      Assert.That(d1, Is.Not.EqualTo(default(TupleDescriptor)));
      Assert.That(d2, Is.Not.EqualTo(default(TupleDescriptor)));
      Assert.That(d2, Is.EqualTo(d1));
      if (theSame!=null)
        Assert.That(d2, Is.EqualTo(theSame));
      Assert.That(d2, Is.EqualTo(d1));
      if (theSame!=null)
        Assert.That(d2, Is.EqualTo(theSame));
      return d1;
    }
  }
}
