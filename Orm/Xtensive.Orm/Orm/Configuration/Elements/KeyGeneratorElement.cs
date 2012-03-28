// Copyright (C) 2012 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2012.03.28

using System.Configuration;
using Xtensive.Configuration;

namespace Xtensive.Orm.Configuration.Elements
{
  /// <summary>
  /// Key generator element within a configuration file.
  /// </summary>
  public class KeyGeneratorElement : ConfigurationCollectionElementBase
  {
    private const string NameElementName = "name";
    private const string SeedElementName = "seed";
    private const string CacheSizeElementName = "cacheSize";

    /// <inheritdoc/>
    public override object Identifier { get { return Name; } }

    /// <summary>
    /// <see cref="KeyGenerator.Name" copy="true"/>
    /// </summary>
    [ConfigurationProperty(NameElementName, IsKey = true)]
    public string Name
    {
      get { return (string) this[NameElementName]; }
      set { this[NameElementName] = value; }
    }

    /// <summary>
    /// <see cref="KeyGenerator.Seed" copy="true"/>
    /// </summary>
    [ConfigurationProperty(SeedElementName, DefaultValue = 0)]
    public long Seed
    {
      get { return (long) this[SeedElementName]; }
      set { this[SeedElementName] = value; }
    }

    /// <summary>
    /// <see cref="KeyGenerator.CacheSize" copy="true" />
    /// </summary>
    [ConfigurationProperty(CacheSizeElementName, DefaultValue = DomainConfiguration.DefaultKeyGeneratorCacheSize)]
    public long CacheSize
    {
      get { return (long) this[CacheSizeElementName]; }
      set { this[CacheSizeElementName] = value; }
    }

    /// <summary>
    /// Converts this instance to corresponding <see cref="KeyGenerator"/>.
    /// </summary>
    /// <returns>Result of conversion.</returns>
    public KeyGenerator ToNative()
    {
      return new KeyGenerator(Name, Seed, CacheSize);
    }
  }
}