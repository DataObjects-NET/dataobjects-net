// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Ivan Galkin
// Created:    2009.04.20

using System;
using Xtensive.Modelling;

namespace Xtensive.Orm.Upgrade.Model
{
  /// <summary>
  /// A collection of <see cref="ForeignKeyColumnRef"/> instances.
  /// </summary>
  [Serializable]
  public sealed class ForeignKeyColumnCollection : NodeCollectionBase<ForeignKeyColumnRef, ForeignKeyInfo>
  {
    /// <summary>
    /// Replaces all column references to references to columns of 
    /// specified <paramref name="source"/> index.
    /// </summary>
    /// <param name="source">The index to use.</param>
    public void Set(StorageIndexInfo source)
    {
      Clear();
      foreach (var keyColumn in source.KeyColumns)
        new ForeignKeyColumnRef(Parent, keyColumn.Value);
    }


    // Constructors

    /// <inheritdoc/>
    public ForeignKeyColumnCollection(Node parent, string name)
      : base(parent, name)
    {
    }
  }
}