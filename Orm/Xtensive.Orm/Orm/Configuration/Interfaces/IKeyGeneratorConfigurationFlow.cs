// Copyright (C) 2012 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2012.03.29

namespace Xtensive.Orm.Configuration
{
  /// <summary>
  /// <see cref="KeyGeneratorConfiguration"/> construction flow.
  /// </summary>
  public interface IKeyGeneratorConfigurationFlow
  {
    /// <summary>
    /// Sets <see cref="KeyGeneratorConfiguration.Seed"/>
    /// for current <see cref="KeyGeneratorConfiguration"/>.
    /// </summary>
    /// <param name="seed">Key generator seed.</param>
    /// <returns>Configuration flow.</returns>
    IKeyGeneratorConfigurationFlow WithSeed(long seed);

    /// <summary>
    /// Sets <see cref="KeyGeneratorConfiguration.CacheSize"/>
    /// for current <see cref="KeyGeneratorConfiguration"/>.
    /// </summary>
    /// <param name="cacheSize">Key generator cache size.</param>
    /// <returns>Configuration flow.</returns>
    IKeyGeneratorConfigurationFlow WithCacheSize(long cacheSize);

    /// <summary>
    /// Sets <see cref="KeyGeneratorConfiguration.Database"/>
    /// for current <see cref="KeyGeneratorConfiguration"/>.
    /// </summary>
    /// <param name="database">Key generator database.</param>
    /// <returns>Configuration flow.</returns>
    IKeyGeneratorConfigurationFlow ForDatabase(string database);
  }
}