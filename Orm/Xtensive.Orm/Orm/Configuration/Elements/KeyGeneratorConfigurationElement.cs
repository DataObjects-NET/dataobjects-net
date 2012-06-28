// Copyright (C) 2012 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2012.03.28

using System.Configuration;
using Xtensive.Core;

namespace Xtensive.Orm.Configuration.Elements
{
  /// <summary>
  /// Key generator element within a configuration file.
  /// </summary>
  public class KeyGeneratorConfigurationElement : ConfigurationCollectionElementBase
  {
    private const string NameElementName = "name";
    private const string SeedElementName = "seed";
    private const string CacheSizeElementName = "cacheSize";
    private const string DatabaseElementName = "database";

    /// <inheritdoc/>
    public override object Identifier {
      get { return new Pair<string>(Name ?? string.Empty, Database ?? string.Empty); }
    }

    /// <summary>
    /// <see cref="KeyGeneratorConfiguration.Name" copy="true"/>
    /// </summary>
    [ConfigurationProperty(NameElementName, IsKey = true)]
    public string Name
    {
      get { return (string) this[NameElementName]; }
      set { this[NameElementName] = value; }
    }

    /// <summary>
    /// <see cref="KeyGeneratorConfiguration.Database" copy="true"/>
    /// </summary>
    [ConfigurationProperty(DatabaseElementName, IsKey = true)]
    public string Database
    {
      get { return (string) this[DatabaseElementName]; }
      set { this[DatabaseElementName] = value; }
    }

    /// <summary>
    /// <see cref="KeyGeneratorConfiguration.Seed" copy="true"/>
    /// </summary>
    [ConfigurationProperty(SeedElementName, DefaultValue = 0)]
    public long Seed
    {
      get { return (long) this[SeedElementName]; }
      set { this[SeedElementName] = value; }
    }

    /// <summary>
    /// <see cref="KeyGeneratorConfiguration.CacheSize" copy="true" />
    /// </summary>
    [ConfigurationProperty(CacheSizeElementName, DefaultValue = DomainConfiguration.DefaultKeyGeneratorCacheSize)]
    public long CacheSize
    {
      get { return (long) this[CacheSizeElementName]; }
      set { this[CacheSizeElementName] = value; }
    }

    /// <summary>
    /// Converts this instance to corresponding <see cref="KeyGeneratorConfiguration"/>.
    /// </summary>
    /// <returns>Result of conversion.</returns>
    public KeyGeneratorConfiguration ToNative()
    {
      return new KeyGeneratorConfiguration(Name) {
        CacheSize = CacheSize,
        Seed = Seed,
        Database = Database,
      };
    }
  }
}