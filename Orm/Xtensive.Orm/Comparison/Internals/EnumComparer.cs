// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: 
// Created:    2008.01.23

using System;
using System.Collections.Generic;
using Xtensive.Core;

using Xtensive.Reflection;

namespace Xtensive.Comparison
{
  [Serializable]
  internal sealed class EnumComparer<TEnum, TSystem>: WrappingComparer<TEnum, TSystem>,
    ISystemComparer<TEnum>
    where TEnum: struct
    where TSystem: struct
  {
    private static readonly Converter<TEnum, TSystem> enumToSystem = DelegateHelper.CreatePrimitiveCastDelegate<TEnum, TSystem>();
    // private static readonly Converter<TSystem, TEnum> systemToEnum = DelegateHelper.CreatePrimitiveCastDelegate<TSystem, TEnum>();
    private readonly TEnum[] values;
    private readonly Dictionary<TEnum, int> valueToIndex;
    private readonly int maxIndex;

    protected override IAdvancedComparer<TEnum> CreateNew(ComparisonRules rules)
    {
      return new EnumComparer<TEnum,TSystem>(Provider, ComparisonRules.Combine(rules));
    }

    public override int Compare(TEnum x, TEnum y)
    {
      return BaseComparer.Compare(enumToSystem(x), enumToSystem(y));
    }

    public override bool Equals(TEnum x, TEnum y)
    {
      return BaseComparer.Equals(enumToSystem(x), enumToSystem(y));
    }

    public override int GetHashCode(TEnum obj)
    {
      return obj.GetHashCode();
    }

    public override TEnum GetNearestValue(TEnum value, Direction direction)
    {
      int index = valueToIndex[value];
      if (index<0)
        throw Exceptions.InvalidArgument(value, "value");
      switch (direction) {
      case Direction.Positive:
        return index >= maxIndex ? value : values[index + 1];
      case Direction.Negative:
        return index == 0 ? value : values[index - 1];
      default:        
        throw Exceptions.InvalidArgument(direction, "direction");
      }
    }


    // Constructors

    public EnumComparer(IComparerProvider provider, ComparisonRules comparisonRules)
      : base(provider, comparisonRules)
    {
      valueToIndex = new Dictionary<TEnum, int>();
      Array originalValues = Enum.GetValues(typeof (TEnum));
      int valueCount = originalValues.Length;
      if (valueCount<1) {
        valueCount = 1;
        values = new TEnum[] {default(TEnum)};
        valueToIndex.Add(values[0], 0);
      }
      else {
        TEnum[] allValues = new TEnum[valueCount];
        for (int i = 0; i < valueCount; i++)
          allValues[i] = (TEnum) originalValues.GetValue(i);
        Array.Sort<TEnum>(allValues, (x, y) => BaseComparer.Compare(enumToSystem(x), enumToSystem(y)));
        for (int i = 0; i<valueCount-1; i++) {
          int j = i + 1;
          if (BaseComparer.Equals(enumToSystem(allValues[i]), enumToSystem(allValues[j]))) {
            valueCount--;
            Array.Copy(allValues, j, allValues, i, valueCount-i);
          }
        }
        values = new TEnum[valueCount];
        Array.Copy(allValues, values, valueCount);
        string[] names = Enum.GetNames(typeof (TEnum));
        for (int i = 0; i<names.Length; i++) {
          TEnum current = (TEnum) originalValues.GetValue(i);
          if (!valueToIndex.ContainsKey(current))
            valueToIndex.Add(current, Array.IndexOf(values, current));
        }
      }
      maxIndex = valueCount - 1;
      ValueRangeInfo = new ValueRangeInfo<TEnum>(
        true, values[0], 
        true, values[valueCount - 1], 
        false, default(TEnum));
    }
  }
}
