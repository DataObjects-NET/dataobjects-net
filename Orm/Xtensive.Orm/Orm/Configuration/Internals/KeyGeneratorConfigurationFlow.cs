// Copyright (C) 2012 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2012.03.29

namespace Xtensive.Orm.Configuration
{
  internal sealed class KeyGeneratorConfigurationFlow : IKeyGeneratorConfigurationFlow
  {
    private readonly KeyGeneratorConfiguration configuration;

    public IKeyGeneratorConfigurationFlow WithSeed(long seed)
    {
      configuration.Seed = seed;
      return this;
    }

    public IKeyGeneratorConfigurationFlow WithCacheSize(long cacheSize)
    {
      configuration.CacheSize = cacheSize;
      return this;
    }

    public IKeyGeneratorConfigurationFlow ForDatabase(string database)
    {
      configuration.Database = database;
      return this;
    }

    // Constructors


    public KeyGeneratorConfigurationFlow(KeyGeneratorConfiguration configuration)
    {
      this.configuration = configuration;
    }
  }
}