// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Aleksey Gamzov
// Created:    2008.08.14

using Xtensive.Core.Collections;
using Xtensive.Core.Helpers;

namespace Xtensive.Sql.Dom.Database.Comparer
{
  public abstract class CompareResult : LockableBase
  {
    private readonly CollectionBaseSlim<PropertyCompareResult> properties = new CollectionBaseSlim<PropertyCompareResult>();

    public CollectionBaseSlim<PropertyCompareResult> Properties
    {
      get { return properties; }
    }

    /// <summary>
    /// Gets <see langword="true"/> if result contains changes, otherwise gets <see langword="false"/>.
    /// </summary>
    public abstract bool HasChanges { get; }

    /// <summary>
    /// Gets result type.
    /// </summary>
    public abstract CompareResultType Result { get; }
  }
}