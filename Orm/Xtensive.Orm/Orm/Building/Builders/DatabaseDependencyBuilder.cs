// Copyright (C) 2012 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2012.03.22

using System;
using System.Collections.Generic;
using System.Linq;
using Xtensive.Core;
using Xtensive.Orm.Configuration;
using Xtensive.Orm.Model;
using Xtensive.Reflection;

namespace Xtensive.Orm.Building.Builders
{
  internal sealed class DatabaseDependencyBuilder
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
        return String.Format("[{0}] {1}.{2} -> [{3}] {4}",
          ownerType.MappingDatabase, ownerType.UnderlyingType.GetShortName(), OwnerField.Name,
          TargetType.MappingDatabase, TargetType.UnderlyingType.GetShortName());
      }

      public TypeReference(FieldInfo ownerField, TypeInfo targetEntry)
      {
        OwnerField = ownerField;
        TargetType = targetEntry;
      }
    }

    private readonly DomainModel model;
    private readonly List<TypeInfo> typesToProcess;
    private readonly HashSet<string> visited = new HashSet<string>();
    private readonly Stack<DatabaseReference> visitSequence = new Stack<DatabaseReference>();
    private Dictionary<string, DatabaseConfiguration> configurationMap;
    private readonly Dictionary<DatabaseReference, TypeReference> referenceRegistry
      = new Dictionary<DatabaseReference, TypeReference>();

    public static void Run(BuildingContext context)
    {
      using (BuildLog.InfoRegion(Strings.LogCalculatingDatabaseDependencies)) {
        new DatabaseDependencyBuilder(context).Run();
      }
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
      var databases = typesToProcess.Select(t => t.MappingDatabase).Distinct().ToList();
      foreach (var database in databases)
        Visit(database);

      // Save calculated reference information in model.
      foreach (var db in databases)
        model.Databases.Add(new DatabaseInfo(db, FindConfiguration(db)));

      foreach (var reference in referenceRegistry.Keys) {
        var owner = model.Databases[reference.OwnerDatabase];
        var target = model.Databases[reference.TargetDatabase];
        owner.ReferencedDatabases.Add(target);
      }
    }

    private DatabaseConfiguration FindConfiguration(string db)
    {
      DatabaseConfiguration result;
      if (configurationMap.TryGetValue(db, out result))
        return result;
      result = new DatabaseConfiguration(db);
      result.Lock();
      return result;
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
          throw new DomainBuilderException(String.Format(
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

    private DatabaseDependencyBuilder(BuildingContext context)
    {
      model = context.Model;
      typesToProcess = model.Types.Where(t => t.IsEntity).ToList();
      configurationMap = context.Configuration.Databases.ToDictionary(db => db.Name);
    }
  }
}