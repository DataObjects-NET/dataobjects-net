// Copyright (C) 2008-2021 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
// Created by: Alex Yakunin
// Created:    2008.01.23

using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Security;
using Xtensive.Core;

using Xtensive.Reflection;

namespace Xtensive.Comparison
{
  [Serializable]
  internal sealed class EnumComparer<TEnum, TSystem> : WrappingComparer<TEnum, TSystem>,
    ISystemComparer<TEnum>
    where TEnum: struct
    where TSystem: struct
  {
    private static readonly Converter<TEnum, TSystem> EnumToSystem = DelegateHelper.CreatePrimitiveCastDelegate<TEnum, TSystem>();

    private readonly TEnum[] values;
    private readonly Dictionary<TEnum, int> valueToIndex;
    private readonly int maxIndex;

    protected override EnumComparer<TEnum, TSystem> CreateNew(ComparisonRules rules)
      => new EnumComparer<TEnum, TSystem>(Provider, ComparisonRules.Combine(rules));

    public override int Compare(TEnum x, TEnum y) => BaseComparer.Compare(EnumToSystem(x), EnumToSystem(y));

    public override bool Equals(TEnum x, TEnum y) => BaseComparer.Equals(EnumToSystem(x), EnumToSystem(y));

    public override int GetHashCode(TEnum obj) => obj.GetHashCode();

    public override TEnum GetNearestValue(TEnum value, Direction direction)
    {
      var index = valueToIndex[value];
      if (index < 0) {
        throw Exceptions.InvalidArgument(value, "value");
      }

      switch (direction) {
        case Direction.Positive:
          return index >= maxIndex ? value : values[index + 1];
        case Direction.Negative:
          return index == 0 ? value : values[index - 1];
        default:
          throw Exceptions.InvalidArgument(direction, "direction");
      }
    }

    private TEnum[] BuildValues(out int valueCount)
    {
      var originalValues = Enum.GetValues(typeof(TEnum));
      valueCount = originalValues.Length;
      if (valueCount < 1) {
        valueCount = 1;
        var values = new TEnum[] { default(TEnum) };
        valueToIndex.Add(values[0], 0);
        return values;
      }
      else {
        var allValues = new TEnum[valueCount];
        for (var i = 0; i < valueCount; i++) {
          allValues[i] = (TEnum) originalValues.GetValue(i);
        }

        Array.Sort<TEnum>(allValues, (x, y) => BaseComparer.Compare(EnumToSystem(x), EnumToSystem(y)));
        for (var i = 0; i < valueCount - 1; i++) {
          var j = i + 1;
          if (BaseComparer.Equals(EnumToSystem(allValues[i]), EnumToSystem(allValues[j]))) {
            valueCount--;
            Array.Copy(allValues, j, allValues, i, valueCount - i);
          }
        }
        var values = new TEnum[valueCount];
        Array.Copy(allValues, values, valueCount);
        var names = Enum.GetNames(typeof(TEnum));
        for (var i = 0; i < names.Length; i++) {
          var current = (TEnum) originalValues.GetValue(i);
          if (!valueToIndex.ContainsKey(current)) {
            valueToIndex.Add(current, Array.IndexOf(values, current));
          }
        }
        return values;
      }
    }


    // Constructors

    public EnumComparer(IComparerProvider provider, ComparisonRules comparisonRules)
      : base(provider, comparisonRules)
    {
      valueToIndex = new Dictionary<TEnum, int>();
      values = BuildValues(out var valueCount);
      maxIndex = valueCount - 1;
      ValueRangeInfo = new ValueRangeInfo<TEnum>(
        true, values[0], 
        true, values[valueCount - 1],
        false, default(TEnum));
    }

    public EnumComparer(SerializationInfo info, StreamingContext context)
      : base(info, context)
    {
      valueToIndex = new Dictionary<TEnum, int>();
      values = BuildValues(out var valueCount);
      maxIndex = valueCount - 1;
      ValueRangeInfo = new ValueRangeInfo<TEnum>(
        true, values[0],
        true, values[valueCount - 1],
        false, default(TEnum));
    }
  }
}
