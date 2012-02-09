// Copyright (C) 2012 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2012.02.08

using System;
using Xtensive.Collections;

namespace Xtensive.Orm.Configuration
{
  /// <summary>
  /// <see cref="DatabaseAlias"/> collection.
  /// </summary>
  public sealed class DatabaseAliasCollection : CollectionBaseSlim<DatabaseAlias>, ICloneable
  {
    /// <summary>
    /// Adds alias with the specified <paramref name="name"/>
    /// targeting specified <paramref name="database"/>.
    /// </summary>
    /// <param name="name">Alias name.</param>
    /// <param name="database">Alias target.</param>
    public void Add(string name, string database)
    {
      Add(new DatabaseAlias(name, database));
    }


    // Constructors

    /// <inheritdoc />
    public object Clone()
    {
      var clone = new DatabaseAliasCollection();
      clone.AddRange(this);
      return clone;
    }
  }
}