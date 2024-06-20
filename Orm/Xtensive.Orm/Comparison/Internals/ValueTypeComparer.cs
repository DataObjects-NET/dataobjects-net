// Copyright (C) 2008-2021 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
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
    protected override ValueTypeComparer<T> CreateNew(ComparisonRules rules)
      => new ValueTypeComparer<T>(Provider, ComparisonRules.Combine(rules));

    public override int Compare(T x, T y)
      => x.CompareTo(y) * DefaultDirectionMultiplier;

    public override bool Equals(T x, T y) => x.Equals(y);

    public override int GetHashCode(T obj) => obj.GetHashCode();

    private void Initialize()
    {
      var valueTypeComparerType = typeof(ValueTypeComparer<T>);
      var myType = GetType();
      var tType = typeof (T);

      var searchFlags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

      var mCompare = myType.GetMethod("Compare", searchFlags,
        null, new Type[] { tType, tType }, null);
      UsesDefaultCompare = mCompare.DeclaringType == valueTypeComparerType;
      var mEquals = myType.GetMethod("Equals", searchFlags,
        null, new Type[] { tType, tType }, null);
      UsesDefaultEquals = mEquals.DeclaringType == valueTypeComparerType;
      var mGetHashCode = myType.GetMethod("GetHashCode", searchFlags,
        null, new Type[] { tType }, null);
      UsesDefaultGetHashCode = mGetHashCode.DeclaringType == valueTypeComparerType;
    }


    // Constructors

    public ValueTypeComparer(IComparerProvider provider, ComparisonRules comparisonRules)
      : base(provider, comparisonRules)
    {
      Initialize();
    }

    public ValueTypeComparer(SerializationInfo info, StreamingContext context)
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
