// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2008.01.29

using System;
using Xtensive.Core.Reflection;
using Xtensive.Core.Threading;
using Xtensive.Core.Tuples;

namespace Xtensive.Core.Comparison
{
  [Serializable]
  internal sealed class TupleComparer : AdvancedComparerBase<Tuple>,
    ISystemComparer<Tuple>
  {
    [NonSerialized]
    private int nullHashCode;
    [NonSerialized]
    private AdvancedComparer<TupleDescriptor> descriptorComparer;
    [NonSerialized]
    private ThreadSafeList<Pair<int, Func<object,object,int>>[]> comparersInfo;


    protected override IAdvancedComparer<Tuple> CreateNew(ComparisonRules rules)
    {
      return new TupleComparer(Provider, ComparisonRules.Combine(rules));
    }

    public override int Compare(Tuple x, Tuple y)
    {
      if (x==null) {
        if (y==null)
          return 0;
        return -DefaultDirectionMultiplier;
      }
      if (y==null)
        return DefaultDirectionMultiplier;

      var dx = x.Descriptor;
      var dy = y.Descriptor;

      var length = dx.GetCommonPartLength(dy);
      var comparers = GetComparersInfo(dx);

      for (int fieldIndex = 0; fieldIndex < length; fieldIndex++) {
        var comparerInfo = comparers[fieldIndex];
        if (comparerInfo.First==0) // Direction check
          return 0;

        TupleFieldState xState;
        TupleFieldState yState;
        var xValue = x.GetValue(fieldIndex, out xState);
        var yValue = y.GetValue(fieldIndex, out yState);
        var stateIndex = (int) xState | ((int) yState << 2);
        switch (stateIndex) {
          case 0:
            continue;
          case 1:
            return comparerInfo.First * (fieldIndex + 1);
          case 2:
            continue;
          case 3:
            continue;
          case 4:
            return -comparerInfo.First * (fieldIndex + 1);
          case 5:
            break;
          case 6:
            return -comparerInfo.First * (fieldIndex + 1);
          case 7:
            return -comparerInfo.First * (fieldIndex + 1);
          case 8:
            continue;
          case 9:
            return comparerInfo.First * (fieldIndex + 1);
          case 10:
            continue;
          case 11:
            continue;
          case 12:
            continue;
          case 13:
            return comparerInfo.First * (fieldIndex + 1);
          case 14:
            continue;
          case 15:
            continue;
        }

        int valueComparison = comparerInfo.Second(xValue, yValue);
        if (valueComparison != 0)
          return valueComparison > 0 ? (fieldIndex + 1) : -(fieldIndex + 1);
      }

      if (comparers.Length > length && comparers[length].First == 0) // Direction check
        return 0;

      if (dx == dy)
        return 0;
      return descriptorComparer.Compare(dx, dy);
    }

    public override bool Equals(Tuple x, Tuple y)
    {
      return Compare(x, y) == 0;
    }

    public override int GetHashCode(Tuple obj)
    {
      if (ReferenceEquals(obj, null))
        return nullHashCode;
      return obj.GetHashCode();
    }

    private Pair<int, Func<object, object, int>>[] GetComparersInfo(TupleDescriptor descriptor)
    {
      return comparersInfo.GetValue(descriptor.Identifier, Generator, this, descriptor);
    }

    private static Pair<int, Func<object, object, int>>[] Generator(int indentifier, TupleComparer tupleComparer, TupleDescriptor descriptor) 
    {
      var box = new Box<Pair<int, Func<object, object, int>>[]>(new Pair<int, Func<object, object, int>>[descriptor.Count]);
      ExecutionSequenceHandler<Box<Pair<int, Func<object, object, int>>[]>>[] initializers =
        DelegateHelper.CreateDelegates<ExecutionSequenceHandler<Box<Pair<int, Func<object, object, int>>[]>>>(
          tupleComparer, tupleComparer.GetType(), "InitializeStep", descriptor);
      DelegateHelper.ExecuteDelegates(initializers, ref box, Direction.Positive);
      return box.Value;
    }

// ReSharper disable UnusedMember.Local
    bool InitializeStep<TFieldType>(ref Box<Pair<int, Func<object, object, int>>[]> data, int fieldIndex)
    {
      data.Value[fieldIndex] = new Pair<int, Func<object, object, int>>(
        (int)ComparisonRules.GetDefaultRuleDirection(fieldIndex),
        Provider.GetComparer<TFieldType>().ApplyRules(ComparisonRules[fieldIndex]).Implementation.Compare);
      return false;
    }
// ReSharper restore UnusedMember.Local

    private void Initialize()
    {
      nullHashCode       = SystemComparerStruct<Tuple>.Instance.GetHashCode(null);
      descriptorComparer = Provider.GetComparer<TupleDescriptor>().ApplyRules(ComparisonRules);
      comparersInfo.Initialize(new object());
    }


    // Constructors

    public TupleComparer(IComparerProvider provider, ComparisonRules comparisonRules) 
      : base(provider, comparisonRules)
    {
      Initialize();
    }

    public override void OnDeserialization(object sender)
    {
      base.OnDeserialization(sender);
      Initialize();
    }
  }
}
