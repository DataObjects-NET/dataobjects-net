// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2009.05.26

namespace Xtensive.Collections
{
  /// <summary>
  /// <see cref="ISet"/> related utilities.
  /// </summary>
  /// <typeparam name="TItem">Type of set item.</typeparam>
  public static class SetUtils<TItem>
  {
    private static readonly ReadOnlySet<TItem> emptySet;

    /// <summary>
    /// Gets empty set of items of <typeparamref name="TItem"/> type.
    /// </summary>
    public static ReadOnlySet<TItem> EmptySet {
      get { return emptySet; }
    }


    // Constructors

    static SetUtils()
    {
      emptySet = new ReadOnlySet<TItem>(new SetSlim<TItem>(), false);
    }
  }
}