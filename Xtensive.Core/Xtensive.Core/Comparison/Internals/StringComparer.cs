// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Nick Svetlov
// Created:    2007.11.28

using System;
using System.Globalization;
using System.Runtime.Serialization;
using Xtensive.Core;
using Xtensive.Resources;

namespace Xtensive.Comparison
{
  [Serializable]
  internal sealed class StringComparer: AdvancedComparerBase<string>,
    ISystemComparer<string>
  {
    private static readonly int emptyHash = string.Empty.GetHashCode();
    [NonSerialized]
    private Func<string, string, CompareOptions, int> stringCompare;
    [NonSerialized]
    private Predicate<string, string, CompareOptions> stringIsSuffix;

    protected override IAdvancedComparer<string> CreateNew(ComparisonRules rules)
    {
      return new StringComparer(Provider, ComparisonRules.Combine(rules));
    }

    public override int Compare(string x, string y)
    {
      if (ReferenceEquals(x, y))
        return 0;

      return stringCompare(x, y, CompareOptions.None) * DefaultDirectionMultiplier;
    }

    public override bool Equals(string x, string y)
    {
      if (ReferenceEquals(x, y))
        return true;
      if (ReferenceEquals(x, null))
        return false;
      if (ReferenceEquals(y, null))
        return false;
      if (x.Length != y.Length)
        return false;
      if (x.GetHashCode() != y.GetHashCode())
        return false;
      return stringIsSuffix(x, y, CompareOptions.None);
    }

    public override int GetHashCode(string obj)
    {
      if (ReferenceEquals(obj, null))
        return emptyHash;
      return obj.GetHashCode();
    }

    public override string GetNearestValue(string value, Direction direction)
    {
      if (direction==Direction.None)
        throw Exceptions.InvalidArgument(direction, "direction");
     
      if (direction == Direction.Positive) { 
        // Next value
        if (value == null)
          return string.Empty;
        else
          if (value==string.Empty || value[value.Length - 1] != Char.MaxValue)
            return value + Char.MinValue;
          else // Ends with Char.MaxValue
            if (value.Length == 1)
              return value;
            else
              return value.Substring(0, value.Length - 2) + (char)(value[value.Length - 2] + 1);
      }
      else {
        // Prev value
        if (String.IsNullOrEmpty(value))
          return null;
        else
          if (value[value.Length - 1] != Char.MinValue)
            return value.Substring(0, value.Length - 1) + (char)(value[value.Length - 1] - 1) + Char.MaxValue;      
          else
            return value.Substring(0, value.Length - 1);
      }
    }

    private void Initialize()
    {
      ValueRangeInfo = new ValueRangeInfo<string>(true, null, false, null, false, null);
      CultureInfo culture = ComparisonRules.Value.Culture;
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
    {
      return string.CompareOrdinal(first, second);
    }

    
    // Constructors

    public StringComparer(IComparerProvider provider, ComparisonRules comparisonRules) 
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
