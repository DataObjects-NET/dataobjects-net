// Copyright (C) 2012 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2012.02.13

using System;
using System.Collections.Generic;
using System.Linq;
using Xtensive.Core;
using Xtensive.Orm.Configuration;
using Xtensive.Orm.Model;
using Xtensive.Reflection;

namespace Xtensive.Orm.Building.Builders
{
  internal sealed class StorageMappingValidator
  {
    private readonly DomainModel model;
    private readonly DomainConfiguration configuration;
    private readonly Validator validator;

    public static void Run(BuildingContext context)
    {
      using (BuildLog.InfoRegion(nameof(Strings.LogValidatingMappingConfiguration))) {
        new StorageMappingValidator(context).ValidateAll();
      }
    }

    private void ValidateAll()
    {
      EnsureDefaultsAreValid();

      if (configuration.IsMultidatabase) {
        EnsureMappingDatabaseIsValid();
        EnsureHierarchiesMapToSingleDatabase();
        if (!configuration.MultidatabaseKeys)
          EnsureIntefacesAreImplementedWithinSingleDatabase();
      }

      if (configuration.IsMultischema)
        EnsureMappingSchemaIsValid();
    }

    private void EnsureMappingSchemaIsValid()
    {
      foreach (var type in GetMappedTypes()) {
        var mappingSchema = type.MappingSchema;
        if (string.IsNullOrEmpty(mappingSchema))
          throw new DomainBuilderException(string.Format(
            Strings.ExMultischemaModeIsActiveButNoSchemaSpecifiedForX,
            type.UnderlyingType.GetShortName()));
        validator.ValidateName(mappingSchema, ValidationRule.Schema);
      }
    }

    private void EnsureMappingDatabaseIsValid()
    {
      foreach (var type in GetMappedTypes()) {
        var mappingDatabase = type.MappingDatabase;
        if (string.IsNullOrEmpty(mappingDatabase))
          throw new DomainBuilderException(string.Format(
            Strings.ExMultidatabaseModeIsActiveButNoDatabaseSpecifiedForX,
            type.UnderlyingType.GetShortName()));
        validator.ValidateName(mappingDatabase, ValidationRule.Database);
      }
    }

    private void EnsureIntefacesAreImplementedWithinSingleDatabase()
    {
      foreach (var @interface in model.Types.Where(t => t.IsInterface)) {
        var implementors = @interface.RecursiveImplementors;
        if (implementors.Count == 0) {
          continue; // shouldn't reach here, but it's safer to do check anyway
        }
        var firstImplementor = implementors[0];
        foreach (var implementor in implementors.Skip(1))
          if (firstImplementor.MappingDatabase != implementor.MappingDatabase)
            throw new DomainBuilderException(string.Format(
              Strings.ExInterfaceXIsImplementedByTypesMappedToDifferentDatabasesYZ,
              @interface.UnderlyingType.GetShortName(),
              GetDatabaseMapping(firstImplementor),
              GetDatabaseMapping(implementor)));
      }
    }

    private void EnsureHierarchiesMapToSingleDatabase()
    {
      foreach (var hierarchy in model.Hierarchies) {
        var root = hierarchy.Root;
        var rootDatabase = root.MappingDatabase;
        foreach (var type in hierarchy.Types.Where(t => t!=root))
          if (type.MappingDatabase!=rootDatabase)
            throw new DomainBuilderException(string.Format(
              Strings.ExSingleHierarchyIsMappedToMultipleDatabasesXY,
              GetDatabaseMapping(root), GetDatabaseMapping(type)));
      }
    }

    private void EnsureDefaultsAreValid()
    {
      var hasDefaultSchema = !string.IsNullOrEmpty(configuration.DefaultSchema);
      var hasDefaultDatabase = !string.IsNullOrEmpty(configuration.DefaultDatabase);

      if (configuration.IsMultidatabase) {
        if (!hasDefaultDatabase || !hasDefaultSchema)
          throw new InvalidOperationException(
            Strings.ExDefaultSchemaAndDefaultDatabaseShouldBeSpecifiedWhenMultidatabaseModeIsActive);
        validator.ValidateName(configuration.DefaultDatabase, ValidationRule.Database);
      }

      if (configuration.IsMultischema) {
        if (!hasDefaultSchema)
          throw new InvalidOperationException(
            Strings.ExDefaultSchemaShouldBeSpecifiedWhenMultischemaOrMultidatabaseModeIsActive);
        validator.ValidateName(configuration.DefaultSchema, ValidationRule.Schema);
      }
    }

    private IEnumerable<TypeInfo> GetMappedTypes()
    {
      return model.Types.Where(t => t.IsEntity);
    }

    private static string GetDatabaseMapping(TypeInfo type)
    {
      return $"{type.UnderlyingType.GetShortName()} -> {type.MappingDatabase}";
    }

    // Constructors

    private StorageMappingValidator(BuildingContext context)
    {
      model = context.Model;
      configuration = context.Domain.Configuration;
      validator = context.Validator;
    }
  }
}