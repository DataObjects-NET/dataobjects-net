// Copyright (C) 2012-2024 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
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
    /// <see cref="KeyGeneratorConfiguration.Name" />
    /// </summary>
    [ConfigurationProperty(NameElementName, IsKey = true)]
    public string Name
    {
      get { return (string) this[NameElementName]; }
      set { this[NameElementName] = value; }
    }

    /// <summary>
    /// <see cref="KeyGeneratorConfiguration.Database" />
    /// </summary>
    [ConfigurationProperty(DatabaseElementName, IsKey = true)]
    public string Database
    {
      get { return (string) this[DatabaseElementName]; }
      set { this[DatabaseElementName] = value; }
    }

    /// <summary>
    /// <see cref="KeyGeneratorConfiguration.Seed" />
    /// </summary>
    [ConfigurationProperty(SeedElementName, DefaultValue = 0L)]
    public long Seed
    {
      get { return (long) this[SeedElementName]; }
      set { this[SeedElementName] = value; }
    }

    /// <summary>
    /// <see cref="KeyGeneratorConfiguration.CacheSize" />
    /// </summary>
    [ConfigurationProperty(CacheSizeElementName, DefaultValue = (long) DomainConfiguration.DefaultKeyGeneratorCacheSize)]
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