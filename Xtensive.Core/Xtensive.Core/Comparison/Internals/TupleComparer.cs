// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2008.01.29

using System;
using System.Collections;
using Xtensive.Core.Comparison;
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
    private ThreadSafeList<Pair<int, IComparer>[]> comparersInfo;


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
      Pair<int, IComparer>[] comparers = GetComparersInfo(dx);

      for (int fieldIndex = 0; fieldIndex < length; fieldIndex++) {
        Pair<int, IComparer> comparerInfo = comparers[fieldIndex];
        if (comparerInfo.First==0) // Direction check
          return 0;

        bool xHasNoValue = !x.IsAvailable(fieldIndex);
        bool yHasNoValue = !y.IsAvailable(fieldIndex);
        if (xHasNoValue) {
          if (yHasNoValue)
            continue;
          return -comparerInfo.First * (fieldIndex + 1);
        } 
        if (yHasNoValue)
          return comparerInfo.First * (fieldIndex + 1);

        var fieldComparer = comparerInfo.Second;

        int valueComparison = fieldComparer.Compare(
          x.GetValueOrDefault(fieldIndex),
          y.GetValueOrDefault(fieldIndex));

        if (valueComparison != 0)
          return valueComparison > 0 ? (fieldIndex + 1) : -(fieldIndex + 1);
      }

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

    private Pair<int, IComparer>[] GetComparersInfo(TupleDescriptor descriptor)
    {
      return comparersInfo.GetValue(descriptor.Identifier, Generator, this, descriptor);
    }

    private static Pair<int, IComparer>[] Generator(int indentifier, TupleComparer tupleComparer, TupleDescriptor descriptor) {
      var box = new Box<Pair<int, IComparer>[]>(new Pair<int, IComparer>[descriptor.Count]);
      ExecutionSequenceHandler<Box<Pair<int, IComparer>[]>>[] initializers = 
        DelegateHelper.CreateDelegates<ExecutionSequenceHandler<Box<Pair<int, IComparer>[]>>>(
          tupleComparer, tupleComparer.GetType(), "InitializeStep", descriptor);
      DelegateHelper.ExecuteDelegates(initializers, ref box, Direction.Positive);
      return box.Value;
    }

    bool InitializeStep<TFieldType>(ref Box<Pair<int, IComparer>[]> data, int fieldIndex)
    {
      data.Value[fieldIndex] = new Pair<int, IComparer>(
        (int)ComparisonRules.GetDefaultRuleDirection(fieldIndex),
        Provider.GetComparer<TFieldType>().ApplyRules(ComparisonRules[fieldIndex]).Implementation);
      return false;
    }

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
