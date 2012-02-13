// Copyright (C) 2012 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2012.02.13

using System;
using System.Linq;
using Xtensive.Orm.Configuration;
using Xtensive.Orm.Model;
using Xtensive.Reflection;

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
      EnsureDefaultsAreSpecified();

      if (model.IsMultidatabase) {
        EnsureMappingDatabaseIsSpecified();
        EnsureHierarchiesMapToSingleDatabase();
        EnsureIntefacesAreImplementedWithinSingleDatabase();
        EnsureNoCyclicReferencesBetweenDatabases();
      }

      if (model.IsMultischema)
        EnsureMappingSchemaIsSpecified();
    }

    private void EnsureMappingSchemaIsSpecified()
    {
      foreach (var type in model.Types.Where(t => t.IsEntity))
        if (string.IsNullOrEmpty(type.MappingSchema))
          throw new DomainBuilderException(string.Format(
            Strings.ExMultischemaModeIsActiveButNoSchemaSpecifiedForX,
            type.UnderlyingType.GetShortName()));
    }

    private void EnsureMappingDatabaseIsSpecified()
    {
      foreach (var type in model.Types.Where(t => t.IsEntity))
        if (string.IsNullOrEmpty(type.MappingDatabase))
          throw new DomainBuilderException(string.Format(
            Strings.ExMultidatabaseModeIsActiveButNoDatabaseSpecifiedForX,
            type.UnderlyingType.GetShortName()));
    }

    private void EnsureNoCyclicReferencesBetweenDatabases()
    {
    }

    private void EnsureIntefacesAreImplementedWithinSingleDatabase()
    {
      foreach (var @interface in model.Types.Where(t => t.IsInterface)) {
        var implementors = @interface.GetImplementors(true).ToList();
        if (implementors.Count==0)
          continue; // shouldn't reach here, but it's safer to do check anyway
        var firstImplementor = implementors[0];
        foreach (var implementor in implementors.Skip(1))
          if (firstImplementor.MappingDatabase!=implementor.MappingDatabase)
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

    private void EnsureDefaultsAreSpecified()
    {
      var hasDefaultSchema = !string.IsNullOrEmpty(configuration.DefaultSchema);
      var hasDefaultDatabase = !string.IsNullOrEmpty(configuration.DefaultDatabase);

      if (model.IsMultidatabase && (!hasDefaultDatabase || !hasDefaultSchema))
        throw new InvalidOperationException(
          Strings.ExDefaultSchemaAndDefaultDatabaseShouldBeSpecifiedWhenMultidatabaseModeIsActive);

      if (model.IsMultischema && !hasDefaultSchema)
        throw new InvalidOperationException(
          Strings.ExDefaultSchemaShouldBeSpecifiedWhenMultischemaModeIsActive);
    }

    private static string GetDatabaseMapping(TypeInfo type)
    {
      return string.Format("{0} -> {1}", type.UnderlyingType.GetShortName(), type.MappingDatabase);
    }

    // Constructors

    private StorageMappingValidator(BuildingContext context)
    {
      model = context.Model;
      configuration = context.Domain.Configuration;
    }
  }
}