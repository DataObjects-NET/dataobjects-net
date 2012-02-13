﻿// Copyright (C) 2012 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2012.02.13

using System;
using Xtensive.Orm.Configuration;
using Xtensive.Orm.Model;

namespace Xtensive.Orm.Building.Builders
{
  internal sealed class StorageMappingValidator
  {
    private readonly DomainModel model;
    private readonly DomainConfiguration configuration;

    public static void Run(BuildingContext context)
    {
      using (Log.InfoRegion(Strings.LogValidatingMappingConfiguration)) {
        new StorageMappingValidator(context).ValidateAll();
      }
    }

    private void ValidateAll()
    {
      ValidateDefaults();
    }

    private void ValidateDefaults()
    {
      var hasDefaultSchema = !string.IsNullOrEmpty(configuration.DefaultSchema);
      var hasDefaultDatabase = !string.IsNullOrEmpty(configuration.DefaultDatabase);

      if (model.IsMultidatabase && (!hasDefaultDatabase || !hasDefaultSchema))
        throw new InvalidOperationException(
          Strings.ExDefaultSchemaAndDefaultDatabaseShouldBeSpecifiedWhenMappingRulesForDatabasesAreDefined);

      if (model.IsMultischema && !hasDefaultSchema)
        throw new InvalidOperationException(
          Strings.ExDefaultSchemaShouldBeSpecifiedWhenMappingRulesForSchemasAreDefined);
    }

    // Constructors

    private StorageMappingValidator(BuildingContext context)
    {
      model = context.Model;
      configuration = context.Domain.Configuration;
    }
  }
}