﻿// Copyright (C) 2012 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2012.05.14

namespace Xtensive.Orm.Configuration
{
  internal sealed class DatabaseConfigurationFlow : IDatabaseConfigurationFlow
  {
    private readonly DatabaseConfiguration configuration;

    public IDatabaseConfigurationFlow WithAlias(string alias)
    {
      configuration.Alias = alias;
      return this;
    }

    public IDatabaseConfigurationFlow WithTypeIdSeed(int value)
    {
      configuration.TypeIdSeed = value;
      return this;
    }

    // Constructors

    public DatabaseConfigurationFlow(DatabaseConfiguration configuration)
    {
      this.configuration = configuration;
    }
  }
}