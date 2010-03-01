// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2008.01.25

using System;
using System.Collections.Generic;
using NUnit.Framework;
using Xtensive.Core.Collections;
using Xtensive.Core.Comparison;
using Xtensive.Core.Diagnostics;
using Xtensive.Core.Reflection;
using Xtensive.Core.Testing;
using Xtensive.Core.Tuples;
using Xtensive.Core.Tuples.Internals;

namespace Xtensive.Core.Tests.Tuples
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
      Assert.AreSame(d1, d2);
      if (theSame!=null)
        Assert.AreSame(theSame, d2);
      Assert.AreEqual(d1, d2);
      if (theSame!=null)
        Assert.AreEqual(theSame, d2);
      TestForIntergity(d1);
      return d1;
    }

    private void TestForIntergity(TupleDescriptor d)
    {
      Type dType = d.GetType();
      Log.Info("Descriptor: {0}", dType.GetShortName());
      
      // Generic type arguments test
      if (!dType.IsGenericType) {
        Assert.AreEqual(0, d.Count);
        Assert.AreEqual(typeof(EmptyTupleDescriptor), dType);
      }
      else {
        Type[] dTypeGenericArgs = dType.GetGenericArguments();
        Assert.IsTrue(
          AdvancedComparer<IEnumerable<Type>>.Default.Equals(d, dTypeGenericArgs),
          "Generic arguments should be the same as FieldTypes.");
      }

      // TupleDescriptor.Create(Type descriptorType) should return it back
      TupleDescriptor theSame = TupleDescriptor.Create(d.GetType());
      Assert.AreSame(d, theSame);

      Pair<int, int> actionData = new Pair<int, int>(0, d.Count-1);

      // Execute(actionHandler, fieldIndex) test
      TestTupleActionHandler ah = new TestTupleActionHandler();
      for (int i = 0; i<d.Count; i++)
        Assert.IsFalse(d.Execute(ah, ref actionData, i));
      Assert.IsTrue(
        AdvancedComparer<IEnumerable<Type>>.Default.Equals(d, ah.CalledForTypes),
        "CalledForTypes list should be the same as FieldTypes.");
      
      // Execute(actionHandler, Direction.Positive) test
      ah = new TestTupleActionHandler();
      d.Execute(ah, ref actionData, Direction.Positive);
      Assert.IsTrue(
        AdvancedComparer<IEnumerable<Type>>.Default.Equals(d, ah.CalledForTypes),
        "CalledForTypes list should be the same as FieldTypes.");

      // Execute(actionHandler, Direction.Negative) test
      ah = new TestTupleActionHandler();
      d.Execute(ah, ref actionData, Direction.Negative);
      Assert.IsTrue(
        AdvancedComparer<IEnumerable<Type>>.Default.Equals(d, ListExtensions.Reverse(ah.CalledForTypes)),
        "CalledForTypes list should be the same as FieldTypes.");

      // Execute(functionHandler, fieldIndex) test
      TestTupleFunctionHandler fh = new TestTupleFunctionHandler();
      TestTupleFunctionHandler.TestFunctionData fhd = fh.CreateData(d);
      for (int i = 0; i<d.Count; i++)
        Assert.IsFalse(d.Execute(fh, ref fhd, i));
      Assert.IsTrue(
        AdvancedComparer<IEnumerable<Type>>.Default.Equals(d, fhd.Result),
        "CalledForTypes list should be the same as FieldTypes.");
      
      // Execute(functionHandler, Direction.Positive) test
      Assert.IsTrue(
        AdvancedComparer<IEnumerable<Type>>.Default.Equals(d, fh.Execute(d, Direction.Positive)),
        "CalledForTypes list should be the same as FieldTypes.");

      // Execute(functionHandler, Direction.Negative) test
      Assert.IsTrue(
        AdvancedComparer<IEnumerable<Type>>.Default.Equals(d, fh.Execute(d, Direction.Negative).Reverse()),
        "CalledForTypes list should be the same as FieldTypes.");
    }

    private static bool EmitTest<TActionData>(ITupleActionHandler<TActionData> h, ref TActionData d, int i)
      where TActionData: struct
    {
      return h.Execute<int>(ref d, i);
    }
  }
}