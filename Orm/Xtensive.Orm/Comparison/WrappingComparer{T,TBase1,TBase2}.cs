// Copyright (C) 2008-2021 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
// Created by: Alex Yakunin
// Created:    2008.01.21

using System;
using System.Runtime.Serialization;
using System.Security;
using Xtensive.Core;


namespace Xtensive.Comparison
{
  /// <summary>
  /// Base class for any wrapping <see cref="IAdvancedComparer{T}"/>s.
  /// </summary>
  /// <typeparam name="T">The type to compare.</typeparam>
  /// <typeparam name="TBase1">First base (wrapped) type.</typeparam>
  /// <typeparam name="TBase2">Second base (wrapped) type.</typeparam>
  [Serializable]
  public abstract class WrappingComparer<T, TBase1, TBase2>: AdvancedComparerBase<T>
  {
    /// <summary>
    /// Comparer delegates for <typeparamref name="TBase1"/> type.
    /// </summary>
    protected AdvancedComparerStruct<TBase1> BaseComparer1;

    /// <summary>
    /// Comparer delegates for <typeparamref name="TBase2"/> type.
    /// </summary>
    protected AdvancedComparerStruct<TBase2> BaseComparer2;

    // Constructors

    /// <summary>
    /// Initializes a new instance of this type.
    /// </summary>
    /// <param name="provider">Comparer provider this comparer is bound to.</param>
    /// <param name="comparisonRules">Comparison rules.</param>
    public WrappingComparer(IComparerProvider provider, ComparisonRules comparisonRules)
      : base(provider, comparisonRules)
    {
      ArgumentNullException.ThrowIfNull(provider);
      BaseComparer1 = provider.GetComparer<TBase1>().ApplyRules(comparisonRules[0]);
      BaseComparer2 = provider.GetComparer<TBase2>().ApplyRules(comparisonRules[1]);
    }

    public WrappingComparer(SerializationInfo info, StreamingContext context)
      : base(info, context)
    {
      BaseComparer1 = (AdvancedComparerStruct<TBase1>) info.GetValue(nameof(BaseComparer1), typeof(AdvancedComparerStruct<TBase1>));
      BaseComparer2 = (AdvancedComparerStruct<TBase2>) info.GetValue(nameof(BaseComparer2), typeof(AdvancedComparerStruct<TBase2>));
    }

    [SecurityCritical]
    public override void GetObjectData(SerializationInfo info, StreamingContext context)
    {
      base.GetObjectData(info, context);
      info.AddValue(nameof(BaseComparer1), BaseComparer1, BaseComparer1.GetType());
      info.AddValue(nameof(BaseComparer2), BaseComparer2, BaseComparer2.GetType());
    }
  }
}