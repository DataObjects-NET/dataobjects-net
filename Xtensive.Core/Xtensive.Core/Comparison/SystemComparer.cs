// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2008.02.10

using System;

namespace Xtensive.Comparison
{
  /// <summary>
  /// <see cref="IAdvancedComparer{T}"/> wrapper for system comparers.
  /// </summary>
  /// <typeparam name="T">Type to compare.</typeparam>
  [Serializable]
  public sealed class SystemComparer<T>: AdvancedComparerBase<T>
  {
    /// <summary>
    /// Gets the only instance of this class.
    /// </summary>
    public readonly static IAdvancedComparer<T> Instance = new SystemComparer<T>(SystemComparerProvider.Instance, ComparisonRules.Positive);

    /// <inheritdoc/>
    protected override IAdvancedComparer<T> CreateNew(ComparisonRules rules)
    {
      return new SystemComparer<T>(Provider, ComparisonRules.Combine(rules));
    }

    /// <inheritdoc/>
    public override int Compare(T x, T y)
    {
      return SystemComparerStruct<T>.Instance.Compare(x, y) * DefaultDirectionMultiplier;
    }

    /// <inheritdoc/>
    public override bool Equals(T x, T y)
    {
      return SystemComparerStruct<T>.Instance.Equals(x, y);
    }

    /// <inheritdoc/>
    public override int GetHashCode(T obj)
    {
      return SystemComparerStruct<T>.Instance.GetHashCode(obj);
    }


    // Constructors

    internal SystemComparer(IComparerProvider provider, ComparisonRules comparisonRules)
      : base(provider, comparisonRules)
    {
    }
  }
}
