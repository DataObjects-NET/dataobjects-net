// Copyright (C) 2012 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2012.03.28

using Xtensive.Core;

namespace Xtensive.Orm.Configuration
{
  /// <summary>
  /// Key generator definition.
  /// </summary>
  public sealed class KeyGeneratorConfiguration : LockableBase
  {
    private string name;
    private long seed;
    private long cacheSize;
    private string database;

    /// <summary>
    /// Gets or sets key generator name.
    /// </summary>
    public string Name
    {
      get { return name; }
      set
      {
        ArgumentValidator.EnsureArgumentNotNullOrEmpty(value, "value");
        this.EnsureNotLocked();
        name = value;
      }
    }

    /// <summary>
    /// Gets database for key generator.
    /// </summary>
    public string Database
    {
      get { return database; }
      set
      {
        this.EnsureNotLocked();
        database = value;
      }
    }

    /// <summary>
    /// Gets or sets seed (initial value) for key generator.
    /// </summary>
    public long Seed
    {
      get { return seed; }
      set
      {
        this.EnsureNotLocked();
        seed = value;
      }
    }

    /// <summary>
    /// Gets cache size (increment) for key generator.
    /// </summary>
    public long CacheSize
    {
      get { return cacheSize; }
      set
      {
        ArgumentValidator.EnsureArgumentIsGreaterThan(value, 0, "value");
        this.EnsureNotLocked();
        cacheSize = value;
      }
    }

    /// <summary>
    /// Clones this instance.
    /// </summary>
    /// <returns>Cloned instance.</returns>
    public KeyGeneratorConfiguration Clone()
    {
      return new KeyGeneratorConfiguration(name) {
        seed = seed,
        database = database,
        cacheSize = cacheSize,
      };
    }

    /// <summary>
    /// Creates new instance of this class
    /// </summary>
    /// <param name="name">Name of the key generator.</param>
    public KeyGeneratorConfiguration(string name)
    {
      Name = name;
      Seed = 0;
      CacheSize = DomainConfiguration.DefaultKeyGeneratorCacheSize;
    }
  }
}