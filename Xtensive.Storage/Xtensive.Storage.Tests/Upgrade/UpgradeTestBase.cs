// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Kofman
// Created:    2009.04.30

using System;
using System.Reflection;
using Xtensive.Storage.Building;
using Xtensive.Storage.Building.Builders;
using Xtensive.Storage.Configuration;

namespace Xtensive.Storage.Tests.Upgrade
{
  public class UpgradeTestBase
  {
    protected Domain Domain { get; private set; }

    protected void BuildDomain(SchemaUpgradeMode schemaUpgradeMode, params Type[] persistentTypes)
    {
      BuildDomain(schemaUpgradeMode,
        types => {
          foreach (Type type in persistentTypes)
            types.Register(type);
        });
    }

    protected void BuildDomain(SchemaUpgradeMode schemaUpgradeMode, string modelNamespace)
    {
      BuildDomain(schemaUpgradeMode, 
        types => types.Register(Assembly.GetExecutingAssembly(), modelNamespace));
    }

    private void BuildDomain(SchemaUpgradeMode schemaUpgradeMode, Action<TypeRegistry> typeRegistrator)
    {
      var configuration = DomainConfigurationFactory.Create();
      typeRegistrator.Invoke(configuration.Types);
      configuration.TypeNameProviderType = typeof (SimpleTypeNameProvider);
      Domain = DomainBuilder.BuildDomain(configuration, schemaUpgradeMode);
    }
  }
}