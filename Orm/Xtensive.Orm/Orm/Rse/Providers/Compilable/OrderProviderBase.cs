// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kochetov
// Created:    2008.07.08

using System;
using System.Linq;
using Xtensive.Collections;
using Xtensive.Core;

namespace Xtensive.Orm.Rse.Providers
{
  /// <summary>
  /// Base class for any compilable sorting providers (such as <see cref="SortProvider"/>).
  /// </summary>
  [Serializable]
  public abstract class OrderProviderBase : UnaryProvider
  {
    /// <summary>
    /// Sort order of the index.
    /// </summary>
    public DirectionCollection<int> Order { get; private set; }

    /// <inheritdoc/>
    protected override RecordSetHeader BuildHeader()
    {
      return Source.Header.Sort(Order);
    }

    /// <inheritdoc/>
    protected override string ParametersToString()
    {
      return Order
        .Select(pair => Header.Columns[pair.Key].Name + (pair.Value == Direction.Negative ? " desc" : string.Empty))
        .ToCommaDelimitedString();
    }


    // Constructors

    /// <summary>
    /// Initializes a new instance of this class.
    /// </summary>
    /// <param name="providerType">Provider type.</param>
    /// <param name="source">The <see cref="UnaryProvider.Source"/> property value.</param>
    /// <param name="order">The <see cref="Order"/> property value.</param>
    protected OrderProviderBase(ProviderType providerType, CompilableProvider source, DirectionCollection<int> order)
      : base(providerType, source)
    {
      Order = order;
    }
  }
}
