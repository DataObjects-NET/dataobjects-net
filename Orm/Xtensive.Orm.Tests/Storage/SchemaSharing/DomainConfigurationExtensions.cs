// Copyright (C) 2017 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kulakov
// Created:    2017.03.06

using Xtensive.Orm.Configuration;

namespace Xtensive.Orm.Tests.Storage.SchemaSharing
{
  public static class DomainConfigurationExtensions
  {
    public static DomainConfiguration MakeNodesShareSchema(this DomainConfiguration configuration)
    {
      configuration.ShareStorageSchemaOverNodes = true;
      return configuration;
    }

    public static DomainConfiguration UseRecreate(this DomainConfiguration configuration)
    {
      configuration.UpgradeMode = DomainUpgradeMode.Recreate;
      return configuration;
    }

    public static DomainConfiguration UsePerformSafely(this DomainConfiguration configuration)
    {
      configuration.UpgradeMode = DomainUpgradeMode.PerformSafely;
      return configuration;
    }

    public static NodeConfiguration UseRecreate(this NodeConfiguration configuration)
    {
      configuration.UpgradeMode = DomainUpgradeMode.Recreate;
      return configuration;
    }

    public static NodeConfiguration UsePerformSafely(this NodeConfiguration configuration)
    {
      configuration.UpgradeMode = DomainUpgradeMode.PerformSafely;
      return configuration;
    }
  }
}