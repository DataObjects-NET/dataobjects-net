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
  /// <see cref="DatabaseConfiguration"/> collection.
  /// </summary>
  public sealed class DatabaseConfigurationCollection : CollectionBaseSlim<DatabaseConfiguration>, ICloneable
  {
    /// <summary>
    /// Adds database configuration with the specified <paramref name="name"/>
    /// and starts <see cref="IDatabaseConfigurationFlow"/>.
    /// </summary>
    /// <param name="name">Database name.</param>
    public IDatabaseConfigurationFlow Add(string name)
    {
      var configuration = new DatabaseConfiguration(name);
      Add(configuration);
      return new DatabaseConfigurationFlow(configuration);
    }

    /// <inheritdoc />
    public object Clone()
    {
      var result = new DatabaseConfigurationCollection();
      foreach (var alias in this)
        result.Add(alias.Clone());
      return result;
    }

    /// <inheritdoc/>
    public override void Lock(bool recursive)
    {
      if (recursive)
        foreach (var alias in this)
          alias.Lock(true);

      base.Lock(recursive);
    }
  }
}