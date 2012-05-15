// Copyright (C) 2012 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2012.05.14

namespace Xtensive.Orm.Configuration
{
  internal sealed class DatabaseConfigurationFlow : IDatabaseConfigurationFlow
  {
    private readonly DatabaseConfiguration configuration;

    public IDatabaseConfigurationFlow WithRealName(string realName)
    {
      configuration.RealName = realName;
      return this;
    }

    public IDatabaseConfigurationFlow WithTypeIdRange(int minValue, int maxValue)
    {
      configuration.MinTypeId = minValue;
      configuration.MaxTypeId = maxValue;
      return this;
    }

    // Constructors

    public DatabaseConfigurationFlow(DatabaseConfiguration configuration)
    {
      this.configuration = configuration;
    }
  }
}