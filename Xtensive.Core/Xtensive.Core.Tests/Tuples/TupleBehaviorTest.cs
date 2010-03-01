// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2008.01.24

using System;
using System.Reflection;
using NUnit.Framework;
using Xtensive.Core.Testing;
using Xtensive.Core.Tuples;

namespace Xtensive.Core.Tests.Tuples
{
  [TestFixture]
  public class TupleBehaviorTest : TupleBehaviorTestBase
  {
    [Test]
    public void GetFieldStateTest()
    {
      Tuple tuple = Tuple.Create(typeof (string));
      TupleFieldState fieldState = tuple.GetFieldState(0);
      Assert.IsTrue(Enum.IsDefined(typeof (TupleFieldState), fieldState));
    }

    [Test]
    public void Test()
    {
      base.Test();
    }
    
    enum TestEnum : sbyte
    {
      One = 5,
      Two
    }

    [Test]
    public void EnumGetHashCodeTest()
    {
      var flag = BindingFlags.Instance;
      var tuple = Tuple.Create(flag);
      var clone = tuple.Clone();
      Console.Out.WriteLine((int)flag);
      Assert.AreEqual(tuple,clone);
      var value = tuple.GetValue<BindingFlags>(0);
      var hashCode = tuple.GetHashCode();
    }

    [Test]
    public void BehaviorTest()
    {
      base.BehaviorTest();
    }

    [Test]
    public void EmptyFieldsTest()
    {
      base.EmptyFieldsTest();
    }

    [Test]
    public void RandomTest()
    {
      base.RandomTest();
    }

    [Test]
    public void CopyTest()
    {
      Random random = RandomManager.CreateRandom(SeedVariatorType.CallingMethod);
      int loopCount = 10;
      int offset = random.Next(fieldTypes.Length);
      Type[] sourceTypes;

      Type[] targetTypes = GetTestTypes(loopCount, out sourceTypes, offset);

      Tuple source = Tuple.Create(sourceTypes);
      Tuple target = Tuple.Create(targetTypes);
      Tuple sameTypeTarget = Tuple.Create(sourceTypes);

      PopulateData(sourceTypes, source);
      PopulateData(targetTypes, target);
      PopulateData(sourceTypes, sameTypeTarget);

      int startingIndex = random.Next(fieldTypes.Length * loopCount / 3) + fieldTypes.Length;
      int startingTargetIndex = (random.Next(loopCount / 4) + 1) * fieldTypes.Length + offset + startingIndex % fieldTypes.Length;
      int count = random.Next(fieldTypes.Length * loopCount / 4);
      
      source.CopyTo(target, startingIndex, startingTargetIndex, count);

      AssertAreSame(source, target, startingIndex, startingTargetIndex, count);

      source.CopyTo(sameTypeTarget, source.Count);

      AssertAreSame(source, sameTypeTarget);

    }

    private Type[] GetTestTypes(int loopCount, out Type[] sourceTypes, int offset)
    {
      sourceTypes = new Type[fieldTypes.Length * loopCount];
      Type[] targetTypes = new Type[fieldTypes.Length * loopCount];


      Array.Copy(fieldTypes, 0, targetTypes, 0, fieldTypes.Length);
      Array.Copy(fieldTypes, 0, targetTypes, (loopCount - 1) * fieldTypes.Length, fieldTypes.Length);
      for (int i = 0; i < loopCount; i++) {
        Array.Copy(fieldTypes, 0, sourceTypes, i * fieldTypes.Length, fieldTypes.Length);
        Array.Copy(fieldTypes, 0, targetTypes, i * fieldTypes.Length + offset, i + 1 == loopCount ? 0 : fieldTypes.Length);
      }
      return targetTypes;
    }

    protected override Tuple CreateTestTuple(TupleDescriptor descriptor)
    {
      return Tuple.Create(descriptor);
    }
  }
}