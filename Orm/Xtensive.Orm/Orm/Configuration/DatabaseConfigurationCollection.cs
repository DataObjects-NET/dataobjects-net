// Copyright (C) 2012-2024 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
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
    object ICloneable.Clone() => Clone();

    /// <summary>
    /// Creates a new object that is a copy of the current instance.
    /// </summary>
    /// <returns>A new object that is a copy of this instance.</returns>
    public DatabaseConfigurationCollection Clone()
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