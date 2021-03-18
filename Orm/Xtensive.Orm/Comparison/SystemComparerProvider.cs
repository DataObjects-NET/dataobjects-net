// Copyright (C) 2008-2021 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
// Created by: Alex Yakunin
// Created:    2008.02.10

using System;
using System.Diagnostics;
using System.Runtime.Serialization;

namespace Xtensive.Comparison
{
  /// <summary>
  /// Provides <see cref="AdvancedComparer{T}"/> wrappers for system comparers.
  /// </summary>
  public sealed class SystemComparerProvider: ComparerProvider
  {
    private static readonly SystemComparerProvider instance = new SystemComparerProvider();

    /// <summary>
    /// Gets the only instance of this class.
    /// </summary>
    public static SystemComparerProvider Instance
    {
      [DebuggerStepThrough]
      get { return instance; }
    }

    /// <inheritdoc/>
    protected override TAssociate CreateAssociate<TKey, TAssociate>(out Type foundFor)
    {
      TAssociate associate = base.CreateAssociate<TKey, TAssociate>(out foundFor);
      return associate is ISystemComparer<TKey>
        ? associate
        : (TAssociate) SystemComparer<TKey>.Instance;
    }


    // Constructors

    private SystemComparerProvider()
    {
    }

    public SystemComparerProvider(SerializationInfo info, StreamingContext context)
      : base(info, context)
    {
    }
  }
}