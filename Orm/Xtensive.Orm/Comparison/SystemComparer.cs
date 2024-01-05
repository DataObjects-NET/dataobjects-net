// Copyright (C) 2008-2021 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
// Created by: Alex Yakunin
// Created:    2008.02.10

using System.Runtime.Serialization;

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
      => new SystemComparer<T>(Provider, ComparisonRules.Combine(rules));

    /// <inheritdoc/>
    public override int Compare(T x, T y)
      => SystemComparerStruct<T>.Instance.Compare(x, y) * DefaultDirectionMultiplier;

    /// <inheritdoc/>
    public override bool Equals(T x, T y) => SystemComparerStruct<T>.Instance.Equals(x, y);

    /// <inheritdoc/>
    public override int GetHashCode(T obj) => SystemComparerStruct<T>.Instance.GetHashCode(obj);


    // Constructors

    internal SystemComparer(IComparerProvider provider, ComparisonRules comparisonRules)
      : base(provider, comparisonRules)
    {
    }

    public SystemComparer(SerializationInfo info, StreamingContext context)
      : base(info, context)
    {
    }
  }
}
