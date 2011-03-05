// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2008.01.23

using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.Serialization;

namespace Xtensive.Comparison
{
  // Fall back to Comparer<T>.Default, EqualityComparer<T>.Default
  [Serializable]
  internal class ValueTypeComparer<T>: ValueTypeComparerBase<T>
    where T: struct, IComparable<T>, IEquatable<T>
  {
    protected override IAdvancedComparer<T> CreateNew(ComparisonRules rules)
    {
      return new ValueTypeComparer<T>(Provider, ComparisonRules.Combine(rules));
    }

    public override int Compare(T x, T y)
    {
      return x.CompareTo(y) 
        * DefaultDirectionMultiplier;
    }

    public override bool Equals(T x, T y)
    {
      return x.Equals(y);
    }

    public override int GetHashCode(T obj)
    {
      return obj.GetHashCode();
    }

    private void Initialize()
    {
      Type valueTypeComparerType = typeof (ValueTypeComparer<T>);
      Type myType = GetType();
      Type tType = typeof (T);
      
      MethodInfo mCompare = myType.GetMethod("Compare", 
        BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic,
        null, new Type[] {tType, tType}, null);
      UsesDefaultCompare = mCompare.DeclaringType==valueTypeComparerType;
      MethodInfo mEquals = myType.GetMethod("Equals",
        BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic,
        null, new Type[] {tType, tType}, null);
      UsesDefaultEquals = mEquals.DeclaringType==valueTypeComparerType;
      MethodInfo mGetHashCode = myType.GetMethod("GetHashCode", 
        BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic,
        null, new Type[] {tType}, null);
      UsesDefaultGetHashCode = mGetHashCode.DeclaringType==valueTypeComparerType;
    }


    // Constructors

    public ValueTypeComparer(IComparerProvider provider, ComparisonRules comparisonRules)
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
