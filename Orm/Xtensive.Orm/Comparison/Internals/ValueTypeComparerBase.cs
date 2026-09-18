// Copyright (C) 2008-2026 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
// Created by: Alex Yakunin
// Created:    2008.01.23

using System;
using System.Collections.Generic;

namespace Xtensive.Comparison
{
  // Fall back to Comparer<T>.Default, EqualityComparer<T>.Default
  internal abstract class ValueTypeComparerBase<T>: AdvancedComparerBase<T>
  {
    public bool UsesDefaultCompare;
    public bool UsesDefaultEquals;
    public bool UsesDefaultGetHashCode;


    // Constructors

    public ValueTypeComparerBase(IComparerProvider provider, ComparisonRules comparisonRules)
      : base(provider, comparisonRules)
    {
    }
  }
}
