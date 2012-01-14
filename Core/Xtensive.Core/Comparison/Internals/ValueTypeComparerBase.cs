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
  }
}
