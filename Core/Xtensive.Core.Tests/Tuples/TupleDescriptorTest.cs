// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2008.01.25

using System;
using System.Collections.Generic;
using NUnit.Framework;
using Xtensive.Collections;
using Xtensive.Diagnostics;
using Xtensive.Testing;
using Xtensive.Tuples;
using Tuple = Xtensive.Tuples.Tuple;

namespace Xtensive.Tests.Tuples
{
  [TestFixture]
  public class TupleDescriptorTest
  {
    public static readonly Type[] FieldTypes = new Type[] {
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
      Random r = RandomManager.CreateRandom(SeedVariatorType.CallingMethod);
      
      int count = 100;
      List<Type> types = new List<Type>();
      List<TupleDescriptor> descriptors = new List<TupleDescriptor>();
      using (new Measurement("Creating descriptors {T1}, {T1,T2}, ...", count)) {
        for (int i = 0; i<count; i++) {
          types.Add(typeof (bool));
          descriptors.Add(TupleDescriptor.Create(types));
        }
      }

      count = 1000;
      int maxSize = 10;
      int size;
      using (new Measurement("Creating random descriptors", count)) {
        for (int i = 0; i<count; i++) {
          size = r.Next(maxSize);
          types = new List<Type>(size);
          for (int j = 0; j < size; j++)
            types.Add(FieldTypes[r.Next(FieldTypes.Length)]);
          descriptors.Add(TupleDescriptor.Create(types));
        }
      }

      count = 100000;
      size = 10;
      types = new List<Type>(size);
      for (int j = 0; j < size; j++)
        types.Add(FieldTypes[r.Next(FieldTypes.Length)]);
      using (new Measurement("Creating the same descriptor", count)) {
        for (int i = 0; i<count; i++) {
          descriptors.Add(TupleDescriptor.Create(types));
        }
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

    private TupleDescriptor TestDescriptor(TupleDescriptor theSame, IList<Type> types)
    {
      TupleDescriptor d1 = TupleDescriptor.Create(types);
      TupleDescriptor d2 = TupleDescriptor.Create(types);
      Assert.IsNotNull(d1);
      Assert.IsNotNull(d2);
      Assert.AreEqual(d1, d2);
      if (theSame!=null)
        Assert.AreEqual(theSame, d2);
      Assert.AreEqual(d1, d2);
      if (theSame!=null)
        Assert.AreEqual(theSame, d2);
      return d1;
    }
  }
}