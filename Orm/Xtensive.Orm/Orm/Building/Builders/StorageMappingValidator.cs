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
    private struct DatabaseReference
    {
      public readonly string OwnerDatabase;
      public readonly string TargetDatabase;

      public DatabaseReference(string ownerDatabase, string targetDatabase)
      {
        OwnerDatabase = ownerDatabase;
        TargetDatabase = targetDatabase;
      }
    }

    private struct TypeReference
    {
      public readonly FieldInfo OwnerField;
      public readonly TypeInfo TargetType;

      public DatabaseReference DatabaseReference
      {
        get
        {
          return new DatabaseReference(
            OwnerField.DeclaringType.MappingDatabase, TargetType.MappingDatabase);
        }
      }

      public override string ToString()
      {
        var ownerType = OwnerField.DeclaringType;
        return string.Format("[{0}] {1}.{2} -> [{3}] {4}",
          ownerType.MappingDatabase, ownerType.UnderlyingType.GetShortName(), OwnerField.Name,
          TargetType.MappingDatabase, TargetType.UnderlyingType.GetShortName());
      }

      public TypeReference(FieldInfo ownerField, TypeInfo targetEntry)
      {
        OwnerField = ownerField;
        TargetType = targetEntry;
      }
    }

    private sealed class CycleDetector
    {
      private readonly DomainModel model;
      private readonly List<TypeInfo> typesToProcess;
      private readonly HashSet<string> visited = new HashSet<string>();
      private readonly Stack<DatabaseReference> visitSequence = new Stack<DatabaseReference>();
      private readonly Dictionary<DatabaseReference, TypeReference> referenceRegistry
        = new Dictionary<DatabaseReference, TypeReference>();

      public static void Run(DomainModel model, List<TypeInfo> typesToProcess)
      {
        new CycleDetector(model, typesToProcess).Run();
      }

      private void Run()
      {
        var outgoingReferences = typesToProcess.SelectMany(GetReferencesToExternalDatabases);

        // Calculate cross-database reference information (i.e. build a graph).
        foreach (var reference in outgoingReferences) {
          var dbReference = reference.DatabaseReference;
          if (!referenceRegistry.ContainsKey(dbReference))
            referenceRegistry.Add(dbReference, reference);
        }

        // Use DFS to find cycles.
        // Since number of databases is small, use very inefficient algorithm.
        var databases = typesToProcess.Select(t => t.MappingDatabase).Distinct();
        foreach (var database in databases)
          Visit(database);
      }

      private void Visit(string database)
      {
        if (visited.Contains(database))
          return;
        visited.Add(database);
        var references = referenceRegistry.Keys
          .Where(r => r.OwnerDatabase==database);
        foreach (var reference in references) {
          visitSequence.Push(reference);
          var next = reference.TargetDatabase;
          if (visitSequence.Any(r => r.OwnerDatabase==next)) {
            var cycle = visitSequence
              .Select(i => referenceRegistry[i])
              .ToCommaDelimitedString();
            throw new DomainBuilderException(string.Format(
              Strings.ExCyclicDependencyBetweenDatabasesFoundX, cycle));
          }
          Visit(next);
          visitSequence.Pop();
        }
      }

      private IEnumerable<TypeReference> GetReferencesToExternalDatabases(TypeInfo source)
      {
        var result = source.Fields
          .Where(field => field.IsEntity && field.IsDeclared)
          .Select(field => new TypeReference(field, model.Types[field.ValueType]))
          .Where(reference => reference.TargetType.MappingDatabase!=source.MappingDatabase);
        return result;
      }

      private CycleDetector(DomainModel model, List<TypeInfo> typesToProcess)
      {
        this.model = model;
        this.typesToProcess = typesToProcess;
      }
    }

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
      EnsureDefaultsAreValid();

      if (model.IsMultidatabase) {
        EnsureMappingDatabaseIsValid();
        EnsureHierarchiesMapToSingleDatabase();
        EnsureIntefacesAreImplementedWithinSingleDatabase();
        EnsureNoCyclicReferencesBetweenDatabases();
      }

      if (model.IsMultischema)
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
        Validator.ValidateName(mappingSchema, ValidationRule.Schema);
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
        Validator.ValidateName(mappingDatabase, ValidationRule.Database);
      }
    }

    private void EnsureNoCyclicReferencesBetweenDatabases()
    {
      CycleDetector.Run(model, GetMappedTypes().ToList());
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

    private void EnsureDefaultsAreValid()
    {
      var hasDefaultSchema = !string.IsNullOrEmpty(configuration.DefaultSchema);
      var hasDefaultDatabase = !string.IsNullOrEmpty(configuration.DefaultDatabase);

      if (model.IsMultidatabase) {
        if (!hasDefaultDatabase || !hasDefaultSchema)
          throw new InvalidOperationException(
            Strings.ExDefaultSchemaAndDefaultDatabaseShouldBeSpecifiedWhenMultidatabaseModeIsActive);
        Validator.ValidateName(configuration.DefaultDatabase, ValidationRule.Database);
      }

      if (model.IsMultischema) {
        if (!hasDefaultSchema)
          throw new InvalidOperationException(
            Strings.ExDefaultSchemaShouldBeSpecifiedWhenMultischemaModeIsActive);
        Validator.ValidateName(configuration.DefaultSchema, ValidationRule.Schema);
      }
    }

    private IEnumerable<TypeInfo> GetMappedTypes()
    {
      return model.Types.Where(t => t.IsEntity);
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