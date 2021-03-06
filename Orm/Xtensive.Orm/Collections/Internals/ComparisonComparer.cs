// Copyright (C) 2019-2020 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
// Created by: Denis Kudelin
// Created:    2019.03.01

using System;
using System.Collections.Generic;
using System.Linq;
using Xtensive.Core;

namespace Xtensive.Collections
{
  internal sealed class ComparisonComparer<T> : Comparer<T>
  {
    private readonly Comparison<T> comparison;

    public static new Comparer<T> Create(Comparison<T> comparison)
    {
      ArgumentValidator.EnsureArgumentNotNull(comparison, "comparison");
      return new ComparisonComparer<T>(comparison);
    }

    public override int Compare(T x, T y)
    {
      return comparison(x, y);
    }


    // Constructors

    private ComparisonComparer(Comparison<T> comparison)
    {
      this.comparison = comparison;
    }
  }
}
