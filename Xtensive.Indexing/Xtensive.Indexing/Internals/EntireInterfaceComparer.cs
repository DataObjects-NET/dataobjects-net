// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2008.02.01

using System;
using Xtensive.Core;
using Xtensive.Core.Collections;
using Xtensive.Core.Comparison;
using Xtensive.Core.Reflection;
using Xtensive.Core.Tuples;

namespace Xtensive.Indexing
{
  [Serializable]
  internal sealed class EntireInterfaceComparer<T>: WrappingComparer<IEntire<T>, T>,
    IComparer<IEntire<T>, T>, ISystemComparer<IEntire<T>>
  {
    private const int NA = -2; // NA means values should be compared
    private static readonly sbyte[] entireValueTypeComparisonMatrix = new sbyte[] {
      0 , -1, -1, -1, -1, 0, 0, 0,
      1,  NA, NA, NA, -1, 0, 0, 0,
      1,  NA, NA, NA, -1, 0, 0, 0,
      1,  NA, NA, NA, -1, 0, 0, 0,
      1,   1,  1,  1,  0, 0, 0, 0,
    };

    internal class ComparisonHandler
    {
      public ExecutionSequenceHandler<EntireComparerData<IEntire<T>, IEntire<T>>>[] Handlers;
      public ExecutionSequenceHandler<EntireComparerData<TupleEntire, Tuple>>[] AsymmetricHandlers;
      public Pair<int, object>[] FieldData;

      public ComparisonHandler(TupleDescriptor descriptor)
      {
        Handlers = new ExecutionSequenceHandler<EntireComparerData<IEntire<T>, IEntire<T>>>[descriptor.Count];
        AsymmetricHandlers = new ExecutionSequenceHandler<EntireComparerData<TupleEntire, Tuple>>[descriptor.Count];
        FieldData = new Pair<int, object>[descriptor.Count];
      }
    }

    private static bool genericArgumentIsTuple = typeof(Tuple).IsAssignableFrom(typeof(T));
    [NonSerialized]
    private int nullHashCode;
    [NonSerialized]
    private AdvancedComparerStruct<T> valueComparer;
    [NonSerialized]
    private AdvancedComparer<TupleDescriptor> descriptorComparer;
    [NonSerialized]
    private ThreadSafeList<ComparisonHandler> comparisonHandlers;
    [NonSerialized]
    private object _lock;


    protected override IAdvancedComparer<IEntire<T>> CreateNew(ComparisonRules rules)
    {
      return new EntireInterfaceComparer<T>(Provider, ComparisonRules.Combine(rules));
    }

    public override Func<IEntire<T>, TSecond, int> GetAsymmetric<TSecond>()
    {
      Type type = typeof(TSecond);
      if (typeof(T)!=type)
        throw new NotSupportedException();
      return ((IComparer<IEntire<T>, TSecond>)(object)this).Compare;
    }

    public override int Compare(IEntire<T> x, IEntire<T> y)
    {
      if (x==null) {
        if (y==null)
          return 0;
        else
          return -DefaultDirectionMultiplier;
      }
      else if (y==null)
        return DefaultDirectionMultiplier;

      var dx = x.Descriptor;
      var dy = y.Descriptor;
      if (dx==dy)
        return Compare_SameDescriptor(x, y, dx);
      else if (dx.Count <= dy.Count)
        return Compare_DifferentDescriptors(x, y, dy);
      else
        return -Compare_DifferentDescriptors(y, x, dx);
    }

    private int Compare_SameDescriptor(IEntire<T> x, IEntire<T> y, TupleDescriptor dx)
    {
      var data = new EntireComparerData<IEntire<T>, IEntire<T>>(x, y);
      ComparisonHandler h = GetComparisonHandler(dx);
      data.FieldData = h.FieldData;
      DelegateHelper.ExecuteDelegates(h.Handlers, ref data, Direction.Positive);
      if (data.Result==Int32.MinValue) // There is no result yet
        return 0;
      else
        return data.Result;
    }

    private int Compare_DifferentDescriptors(IEntire<T> x, IEntire<T> y, TupleDescriptor dy)
    {
      var data = new EntireComparerData<IEntire<T>, IEntire<T>>(x, y);
      ComparisonHandler h = GetComparisonHandler(dy);
      data.FieldData = h.FieldData; // Longer Tuple's data
      DelegateHelper.ExecuteDelegates(h.Handlers /* Longer Tuple's handlers */, ref data, Direction.Positive);
      if (data.Result==Int32.MinValue) // There is no result yet
        return -data.X.Count
          * DefaultDirectionMultiplier;
      else
        return data.Result;
    }

    public int Compare(IEntire<T> x, T y)
    {
      if (genericArgumentIsTuple)
        return Compare_WithTuple(x as TupleEntire, y as Tuple);
      else
        return Compare_WithValue(x, y);
    }

    private int Compare_WithTuple(TupleEntire x, Tuple y)
    {
      if (x==null) {
        if (y==null)
          return 0;
        else
          return -DefaultDirectionMultiplier;
      }
      else if (y==null)
        return DefaultDirectionMultiplier;

      var dx = x.Descriptor;
      var dy = y.Descriptor;
      var data = new EntireComparerData<TupleEntire, Tuple>(x, y);
      ComparisonHandler h = GetComparisonHandler(dy);
      if (dx==dy)
        data.Result = 0; // Sizes are the same, so result is known on completion
      data.FieldData = h.FieldData; // Entire's data
      DelegateHelper.ExecuteDelegates(h.AsymmetricHandlers /* Entire's handlers */, ref data, Direction.Positive);
      if (data.Result==Int32.MinValue) // There is no result yet, but sizes are different
        return -data.X.Count
          * DefaultDirectionMultiplier;
      else
        return data.Result;
    }

    private int Compare_WithValue(IEntire<T> x, T y)
    {
      EntireValueType xValueType = x.GetValueType(0);
      switch (xValueType) {
      case EntireValueType.NegativeInfinity:
        return -DefaultDirectionMultiplier;
      case EntireValueType.PositiveInfinity:
        return DefaultDirectionMultiplier;
      }

      int result = valueComparer.Compare(x.GetValueOrDefault<T>(0), y);
      if (result!=0)
        return result;
      return (int) xValueType 
        * DefaultDirectionMultiplier;
    }

    public override bool Equals(IEntire<T> x, IEntire<T> y)
    {
      return Compare(x, y) == 0;
    }

    public override int GetHashCode(IEntire<T> obj)
    {
      if (ReferenceEquals(obj, null))
        return nullHashCode;
      else
        return obj.GetHashCode();
    }

    private ComparisonHandler GetComparisonHandler(TupleDescriptor descriptor)
    {
      int identifier = descriptor.Identifier;
      ComparisonHandler h = comparisonHandlers.GetValue(identifier);
      if (h==null)
        lock (_lock) {
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

    #region InitializeStep, CompareStep, AssymetricCompareStep

    bool InitializeStep<TFieldType>(ref Box<ComparisonHandler> data, int fieldIndex)
    {
      data.Value.Handlers[fieldIndex] = CompareStep<TFieldType>;
      data.Value.AsymmetricHandlers[fieldIndex] = AsymmetricCompareStep<TFieldType>;
      data.Value.FieldData[fieldIndex] = new Pair<int, object>(
        (int)ComparisonRules.GetDefaultRuleDirection(fieldIndex),
        Provider.GetComparer<TFieldType>().ApplyRules(ComparisonRules[fieldIndex]));
      return false;
    }

    private bool CompareStep<TFieldType>(ref EntireComparerData<IEntire<T>, IEntire<T>> data, int fieldIndex)
    {
      Pair<int, object> fieldData = data.FieldData[fieldIndex];
      if (fieldData.First == 0) { // Direction check
        data.Result = 0;
        return true;
      }
      if (data.XCount==fieldIndex)
        return true;

      EntireValueType xValueType = data.X.GetValueType(fieldIndex);
      EntireValueType yValueType = data.Y.GetValueType(fieldIndex);

      // Infinities comparison
      if (xValueType==EntireValueType.NegativeInfinity) {
        if (yValueType==EntireValueType.NegativeInfinity) {
          data.Result = 0;
          return true;
        }
        else {
          data.Result = -fieldData.First * (fieldIndex+1);
          return true;
        }
      }
      else if (xValueType==EntireValueType.PositiveInfinity) {
        if (yValueType==EntireValueType.PositiveInfinity) {
          data.Result = 0;
          return true;
        }
        else {
          data.Result = fieldData.First * (fieldIndex+1);
          return true;
        }
      }
      else if (yValueType==EntireValueType.NegativeInfinity) {
        data.Result = fieldData.First * (fieldIndex+1);
        return true;
      }
      else if (yValueType==EntireValueType.PositiveInfinity) {
        data.Result = -fieldData.First * (fieldIndex+1);
        return true;
      }
//      int result = entireValueTypeComparisonMatrix[((2 + (int)xValueType) << 3) + (2 + (int)yValueType)];
//      if (result!=NA) {
//        data.Result = result * fieldData.First;
//        return true;
//      }

      // HasValue comparison
      bool xHasNoValue = !data.X.HasValue(fieldIndex);
      bool yHasNoValue = !data.Y.HasValue(fieldIndex);
      if (xHasNoValue && yHasNoValue)
        return false;
      if (xHasNoValue) {
        data.Result = -fieldData.First * (fieldIndex+1);
        return true;
      }
      if (yHasNoValue) {
        data.Result = fieldData.First * (fieldIndex+1);
        return true;
      }

      // Value comparison
      var fieldComparer = fieldData.Second as AdvancedComparer<TFieldType>;
      if (fieldComparer==null) {
        // Field types are different
        data.Result = descriptorComparer.Compare(data.X.Descriptor, data.Y.Descriptor);
        return true;
      }
      int result = fieldComparer.Compare(
        data.X.GetValueOrDefault<TFieldType>(fieldIndex),
        data.Y.GetValueOrDefault<TFieldType>(fieldIndex));
      if (result!=0) {
        data.Result = result>0 ? (fieldIndex+1) : -(fieldIndex+1);
        return true;
      }

      // Infinitesimal shift comparison
      result = (int) xValueType - (int) yValueType;
      if (result!=0) {
        data.Result = result * fieldData.First * (fieldIndex+1);
        return true;
      }
      return false;
    }

    private bool AsymmetricCompareStep<TFieldType>(ref EntireComparerData<TupleEntire, Tuple> data, int fieldIndex)
    {
      Pair<int, object> fieldData = data.FieldData[fieldIndex];
      if (fieldData.First == 0) { // Direction check
        data.Result = 0;
        return true;
      }

      if (data.X.Count==fieldIndex)
        return true;

      // Infinity check
      EntireValueType xValueType = data.X.GetValueType(fieldIndex);
      switch (xValueType) {
      case EntireValueType.NegativeInfinity:
        data.Result = -fieldData.First * (fieldIndex+1);
        return true;
      case EntireValueType.PositiveInfinity:
        data.Result = fieldData.First * (fieldIndex+1);
        return true;
      }

      // HasValue check
      bool xHasNoValue = !data.X.IsAvailable(fieldIndex);
      bool yHasNoValue = !data.Y.IsAvailable(fieldIndex);
      if (xHasNoValue && yHasNoValue)
        return false;
      if (xHasNoValue) {
        data.Result = -fieldData.First * (fieldIndex+1);
        return true;
      }
      if (yHasNoValue) {
        data.Result = fieldData.First * (fieldIndex+1);
        return true;
      }

      var fieldComparer = (AdvancedComparer<TFieldType>)fieldData.Second;
      if (fieldComparer==null) {
        // Field types are different
        data.Result = descriptorComparer.Compare(data.X.Descriptor, data.Y.Descriptor);
        return true;
      }
      int result = fieldComparer.Compare(
        data.X.GetValueOrDefault<TFieldType>(fieldIndex),
        data.Y.GetValueOrDefault<TFieldType>(fieldIndex));
      if (result!=0) {
        data.Result = result>0 ? (fieldIndex+1) : -(fieldIndex+1);
        return true;
      }

      // Infinitesimal shift check
      result = (int) xValueType;
      if (result!=0) {
        data.Result = result * fieldData.First * (fieldIndex+1);
        return true;
      }
      return false;
    }

    #endregion

    private void Initialize()
    {
      _lock = new object();
      nullHashCode   = SystemComparer<Tuple>.Instance.GetHashCode(null);
      valueComparer  = Provider.GetComparer<T>().ApplyRules(ComparisonRules);
      descriptorComparer = Provider.GetComparer<TupleDescriptor>().ApplyRules(ComparisonRules);
      comparisonHandlers.Initialize();
    }


    // Constructors

    public EntireInterfaceComparer(IComparerProvider provider, ComparisonRules comparisonRules)
      : base(provider, comparisonRules)
    {
      ValueRangeInfo<T> baseValueRangeInfo = BaseComparer.ValueRangeInfo;
      IEntire<T> min = Entire<T>.MinValue;
      IEntire<T> max = Entire<T>.MaxValue;
      ValueRangeInfo = new ValueRangeInfo<IEntire<T>>(
        true, min,
        true, max,
        baseValueRangeInfo.HasDeltaValue,
        baseValueRangeInfo.HasDeltaValue ? Entire<T>.Create(baseValueRangeInfo.DeltaValue) : Entire<T>.Create(default(T)));
      Initialize();
    }

    public override void OnDeserialization(object sender)
    {
      base.OnDeserialization(sender);
      Initialize();
    }
  }
}