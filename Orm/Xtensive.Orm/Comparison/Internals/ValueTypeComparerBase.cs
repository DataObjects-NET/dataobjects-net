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
  internal abstract class ValueTypeComparerBase<T>: AdvancedComparerBase<T>
  {
    [NonSerialized]
    public bool UsesDefaultCompare;
    [NonSerialized]
    public bool UsesDefaultEquals;
    [NonSerialized]
    public bool UsesDefaultGetHashCode;


    // Constructors

    public ValueTypeComparerBase(IComparerProvider provider, ComparisonRules comparisonRules)
      : base(provider, comparisonRules)
    {
    }

    public ValueTypeComparerBase(SerializationInfo info, StreamingContext context)
      : base(info, context)
    {
    }
  }
}
