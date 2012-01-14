// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2008.02.10

using System;
using System.Diagnostics;

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
      if (associate is ISystemComparer<TKey>)
        return associate;
      else
        return (TAssociate) SystemComparer<TKey>.Instance;
    }


    // Constructors

    private SystemComparerProvider()
    {
    }
  }
}