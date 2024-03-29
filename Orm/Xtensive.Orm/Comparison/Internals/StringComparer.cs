// Copyright (C) 2007-2021 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
// Created by: Nick Svetlov
// Created:    2007.11.28

using System;
using System.Globalization;
using System.Runtime.Serialization;
using Xtensive.Core;


namespace Xtensive.Comparison
{
  [Serializable]
  internal sealed class StringComparer: AdvancedComparerBase<string>,
    ISystemComparer<string>
  {
    private static readonly int EmptyHash = string.Empty.GetHashCode();
    [NonSerialized]
    private Func<string, string, CompareOptions, int> stringCompare;
    [NonSerialized]
    private Func<string, string, CompareOptions, bool> stringIsSuffix;

    protected override IAdvancedComparer<string> CreateNew(ComparisonRules rules)
    {
      return new StringComparer(Provider, ComparisonRules.Combine(rules));
    }

    public override int Compare(string x, string y)
    {
      return ReferenceEquals(x, y)
        ? 0
        : stringCompare(x, y, CompareOptions.None) * DefaultDirectionMultiplier;
    }

    public override bool Equals(string x, string y)
    {
      if (ReferenceEquals(x, y)) {
        return true;
      }
      if (ReferenceEquals(x, null) || ReferenceEquals(y, null)) {
        return false;
      }
      if (x.Length != y.Length) {
        return false;
      }
      return x.GetHashCode() != y.GetHashCode()
        ? false
        : stringIsSuffix(x, y, CompareOptions.None);
    }

    public override int GetHashCode(string obj)
      => obj is null ? EmptyHash : obj.GetHashCode();

    public override string GetNearestValue(string value, Direction direction)
    {
      if (direction == Direction.None) {
        throw Exceptions.InvalidArgument(direction, "direction");
      }

      if (direction == Direction.Positive) {
        // Next value
        if (value == null) {
          return string.Empty;
        }
        else {
          if (value == string.Empty || value[value.Length - 1] != char.MaxValue) {
            return value + char.MinValue;
          }
          else { // Ends with Char.MaxValue
            return value.Length == 1
              ? value
              : value.Substring(0, value.Length - 2) + (char) (value[value.Length - 2] + 1);
          }
        }
      }
      else {
        // Prev value
        if (string.IsNullOrEmpty(value)) {
          return null;
        }
        else {
          return value[value.Length - 1] != char.MinValue
            ? value.Substring(0, value.Length - 1) + (char) (value[value.Length - 1] - 1) + char.MaxValue
            : value.Substring(0, value.Length - 1);
        }
      }
    }

    private void Initialize()
    {
      ValueRangeInfo = new ValueRangeInfo<string>(true, null, false, null, false, null);
      var culture = ComparisonRules.Value.Culture;
      if (culture != null) {
        stringCompare  = culture.CompareInfo.Compare;
        stringIsSuffix = culture.CompareInfo.IsSuffix;
      }
      else {
        stringCompare  = CompareOrdinal;
        stringIsSuffix = CultureInfo.InvariantCulture.CompareInfo.IsSuffix;
      }
    }

    private static int CompareOrdinal(string first, string second, CompareOptions options)
      => string.CompareOrdinal(first, second);


    // Constructors

    public StringComparer(IComparerProvider provider, ComparisonRules comparisonRules) 
      : base(provider, comparisonRules)
    {
      Initialize();
    }

    public StringComparer(SerializationInfo info, StreamingContext context)
      : base(info, context)
    {
    }

    public override void OnDeserialization(object sender)
    {
      base.OnDeserialization(sender);
      Initialize();
    }
  }
}
