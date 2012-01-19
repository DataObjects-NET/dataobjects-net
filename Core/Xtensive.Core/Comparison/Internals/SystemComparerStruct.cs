// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2008.02.10

using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Xtensive.Core;

namespace Xtensive.Comparison
{
  internal struct SystemComparerStruct<T>
  {
    public readonly static SystemComparerStruct<T> Instance = new SystemComparerStruct<T>(true);

    public readonly Func<T, T, int> Compare;
    public readonly new Predicate<T, T> Equals;
    public readonly new Func<T, int> GetHashCode;

    
    // Constructors
    
    private SystemComparerStruct(bool ignore)
    {
      IComparer<T> comparer = 
        (IComparer<T>)Comparer<T>.Default ?? 
        (IComparer<T>)(object)NoSystemComparerHandler<T>.Instance;
      IEqualityComparer<T> equalityComparer = 
        (IEqualityComparer<T>)EqualityComparer<T>.Default ?? 
        (IEqualityComparer<T>)(object)NoSystemComparerHandler<T>.Instance;
      Compare = comparer.Compare;
      Equals = equalityComparer.Equals;
      GetHashCode = equalityComparer.GetHashCode;
    }
  }
}