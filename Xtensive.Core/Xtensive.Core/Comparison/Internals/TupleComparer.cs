// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2008.01.29

using System;
using Xtensive.Core.Collections;
using Xtensive.Core.Comparison;
using Xtensive.Core.Reflection;
using Xtensive.Core.Tuples;
using Xtensive.Core.Tuples.Internals;

namespace Xtensive.Core.Comparison
{
  [Serializable]
  internal sealed class TupleComparer : AdvancedComparerBase<Tuple>,
    ISystemComparer<Tuple>
  {
    internal class ComparisonHandler
    {
      public ExecutionSequenceHandler<TupleComparerData>[] Handlers;
      public Pair<int, object>[] FieldData;

      public ComparisonHandler(TupleDescriptor descriptor)
      {
        Handlers  = new ExecutionSequenceHandler<TupleComparerData>[descriptor.Count];
        FieldData = new Pair<int, object>[descriptor.Count];
      }
    }

    [NonSerialized]
    private int nullHashCode;
    [NonSerialized]
    private AdvancedComparer<TupleDescriptor> descriptorComparer;
    [NonSerialized]
    private ThreadSafeList<ComparisonHandler> comparisonHandlers;


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

      if (dx==dy) {
        var data = new TupleComparerData(x, y);
        ComparisonHandler h = GetComparisonHandler(dx);
        data.FieldData = h.FieldData;
        DelegateHelper.ExecuteDelegates(h.Handlers, ref data, Direction.Positive);
        if (data.Result==Int32.MinValue) // There is no result yet
          return 0;
        return data.Result;
      }
      if (dx.Count<=dy.Count)
        return Compare_DifferentDescriptors(x, y, dx, dy);
      return -Compare_DifferentDescriptors(y, x, dy, dx);
    }

    private int Compare_DifferentDescriptors(Tuple x, Tuple y, TupleDescriptor dx, TupleDescriptor dy)
    {
      var data = new TupleComparerData(x, y);
      ComparisonHandler hx = GetComparisonHandler(dx);
      ComparisonHandler hy = GetComparisonHandler(dy);
      data.FieldData = hy.FieldData; // Longer Tuple's data
      DelegateHelper.ExecuteDelegates(hx.Handlers /* Shorter Tuple's handlers */, ref data, Direction.Positive);
      if (data.Result==Int32.MinValue) {
        // There is no result yet 
        int count = data.X.Count;
        if (data.FieldData[count].First==0) // And next direction to compare is none
          return 0;
        return -data.X.Count * DefaultDirectionMultiplier;
      }
      return data.Result;
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

    private ComparisonHandler GetComparisonHandler(TupleDescriptor descriptor)
    {
      int identifier = descriptor.Identifier;
      ComparisonHandler h = comparisonHandlers.GetValue(identifier);
      if (h==null) lock (this) {
        h = comparisonHandlers.GetValue(identifier);
        if (h==null) {
          var box = new Box<ComparisonHandler>(new ComparisonHandler(descriptor));
          ExecutionSequenceHandler<Box<ComparisonHandler>>[] initializers =
            DelegateHelper.CreateDelegates<ExecutionSequenceHandler<Box<ComparisonHandler>>>(
              this, GetType(), "InitializeStep", descriptor);
          DelegateHelper.ExecuteDelegates(initializers, ref box, Direction.Positive);
          h = box.Value;
          comparisonHandlers.SetValue(identifier, h);
        }
      }
      return h;
    }

    #region Initialize\Compare\Equals steps

    bool InitializeStep<TFieldType>(ref Box<ComparisonHandler> data, int fieldIndex)
    {
      data.Value.Handlers[fieldIndex] = CompareStep<TFieldType>;
      data.Value.FieldData[fieldIndex] = new Pair<int, object>(
        (int)ComparisonRules.GetDefaultRuleDirection(fieldIndex),
        Provider.GetComparer<TFieldType>().ApplyRules(ComparisonRules[fieldIndex]));
      return false;
    }

    bool CompareStep<TFieldType>(ref TupleComparerData data, int fieldIndex)
    {
      Pair<int, object> fieldData = data.FieldData[fieldIndex];
      if (fieldData.First == 0) { // Direction check
        data.Result = 0;
        return true;
      }

      if (data.X.Count == fieldIndex)
        return true;
    
      bool xHasNoValue = !data.X.IsAvailable(fieldIndex);
      bool yHasNoValue = !data.Y.IsAvailable(fieldIndex);
      if (xHasNoValue) {
        if (yHasNoValue)
          return false;
        data.Result = -fieldData.First * (fieldIndex+1);
        return true;
      }
      if (yHasNoValue) {
        data.Result = fieldData.First * (fieldIndex+1);
        return true;
      }

      var fieldComparer = fieldData.Second as AdvancedComparer<TFieldType>;
      if (fieldComparer==null) {
        // Field types are different
        data.Result = descriptorComparer.Compare(data.X.Descriptor, data.Y.Descriptor);
        return true;
      }
      int result = fieldComparer.Compare( // May throw NullReferenceException - but it is always processed by caller
        data.X.GetValueOrDefault<TFieldType>(fieldIndex),
        data.Y.GetValueOrDefault<TFieldType>(fieldIndex));
      if (result!=0) {
        data.Result = result>0 ? (fieldIndex+1) : -(fieldIndex+1);
        return true;
      }
      return false;
    }

    #endregion

    private void Initialize()
    {
      nullHashCode       = SystemComparerStruct<Tuple>.Instance.GetHashCode(null);
      descriptorComparer = Provider.GetComparer<TupleDescriptor>().ApplyRules(ComparisonRules);
      comparisonHandlers.Initialize();
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