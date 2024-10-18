// Copyright (C) 2024 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.

using System;
using System.Collections.Generic;
using Xtensive.Core;

namespace Xtensive.Orm.Configuration.Options
{
  internal sealed class KeyGeneratorOptions : IIdentifyableOptions,
    IValidatableOptions,
    IHasDatabaseOption,
    IToNativeConvertable<KeyGeneratorConfiguration>
  {
    public object Identifier => (Name ?? string.Empty, Database ?? string.Empty);

    /// <summary>
    /// Key generator name.
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// Database for key generator
    /// </summary>
    public string Database { get; set; }

    /// <summary>
    /// Seed (initial value) for key generator.
    /// </summary>
    public long Seed { get; set; } = 0L;

    /// <summary>
    /// Cache size (increment) for cache generator.
    /// </summary>
    public long CacheSize { get; set; } =  DomainConfiguration.DefaultKeyGeneratorCacheSize;

    public object GetMappedIdentifier(IDictionary<string, DatabaseOptions> databaseMap)
    {
      if (!Database.IsNullOrEmpty() && databaseMap.TryGetValue(Database, out var map) && !map.RealName.IsNullOrEmpty()) {
        return(Name ?? string.Empty, map.RealName);
      }
      return Identifier;
    }

    /// <inheritdoc/>
    /// <exception cref="ArgumentException">Name is null or empty.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Seed or CacheSize value is out of valid range.</exception>
    public void Validate()
    {
      if (Name.IsNullOrEmpty()) {
        throw new ArgumentException("Key generator should have not empty name.");
      }
      if (Seed < 0) {
        throw new ArgumentOutOfRangeException("Key generator seed must be non-negative value");
      }
      if (CacheSize <= 0) {
        throw new ArgumentOutOfRangeException("Key generator cache size must be positive value");
      }
    }

    /// <inheritdoc />
    public KeyGeneratorConfiguration ToNative()
    {
      Validate();

      return new KeyGeneratorConfiguration(Name) {
        CacheSize = CacheSize,
        Seed = Seed,
        Database = Database,
      };
    }

  }
}
