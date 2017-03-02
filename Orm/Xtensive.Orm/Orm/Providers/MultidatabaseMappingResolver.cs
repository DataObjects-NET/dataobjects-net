// Copyright (C) 2014 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2014.03.13

using System;
using System.Collections.Generic;
using System.Linq;
using Xtensive.Orm.Configuration;
using Xtensive.Orm.Upgrade;
using Xtensive.Sql;
using Xtensive.Sql.Model;

namespace Xtensive.Orm.Providers
{
  internal sealed class MultidatabaseMappingResolver : MappingResolver
  {
    private readonly string defaultDatabase;
    private readonly string defaultSchema;

    private readonly List<SqlExtractionTask> extractionTasks;
    private readonly List<SqlExtractionTask> metadataTasks;

    private readonly NameMappingCollection databaseMapping;
    private readonly NameMappingCollection schemaMapping;
    private readonly NameMappingCollection reversedDatabaseMapping;
    private readonly NameMappingCollection reversedSchemaMapping;

    public override string GetNodeName(string mappingDatabase, string mappingSchema, string mappingName)
    {
      // The only reason this method accepts empty database or schema is HintGenerator.
      // It needs resolving mapping information from previous upgrade
      // which could be performed in different multidatabase/multischema mode.
      // If such case occurs we simply use current domain defaults as mapping information.

      if (string.IsNullOrEmpty(mappingDatabase))
        mappingDatabase = defaultDatabase;

      if (string.IsNullOrEmpty(mappingSchema))
        mappingSchema = defaultSchema;

      return FormatNodeName(
        databaseMapping.Apply(mappingDatabase),
        schemaMapping.Apply(mappingSchema),
        mappingName);
    }

    public override string GetNodeName(SchemaNode node)
    {
      var schema = node.Schema;
      return FormatNodeName(schema.Catalog.Name, schema.Name, node.Name);
    }

    public override MappingResolveResult Resolve(SchemaExtractionResult model, string nodeName)
    {
      var names = nodeName.Split(NameElementSeparator);
      var catalog = (model.IsShared) 
        ? model.Catalogs[reversedDatabaseMapping.Apply(names[0])]
        : model.Catalogs[names[0]];
      if (catalog==null)
        throw new InvalidOperationException(string.Format(Strings.ExUnableToResolveDatabaseForNodeXPleaseVerifyThatThisDatabaseExists, nodeName));
      var schema = (model.IsShared)
        ? catalog.Schemas[reversedSchemaMapping.Apply(names[1])]
        : catalog.Schemas[names[1]];
      if (schema==null)
        throw new InvalidOperationException(string.Format(Strings.ExUnableToResolveSchemaForNodeXPleaseVerifyThatThisSchemaExists, nodeName));
      var name = names[2];
      return new MappingResolveResult(schema, name);
    }

    public override IEnumerable<SqlExtractionTask> GetSchemaTasks()
    {
      return extractionTasks;
    }

    public override IEnumerable<SqlExtractionTask> GetMetadataTasks()
    {
      return metadataTasks;
    }

    private static IEnumerable<string> GetSchemasForDatabase(DomainConfiguration configuration, string database)
    {
      var userSchemas =
        from rule in configuration.MappingRules
        let db = string.IsNullOrEmpty(rule.Database) ? configuration.DefaultDatabase : rule.Database
        where db==database
        select string.IsNullOrEmpty(rule.Schema) ? configuration.DefaultSchema : rule.Schema;

      return userSchemas
        .Concat(Enumerable.Repeat(configuration.DefaultSchema, 1))
        .Distinct();
    }

    private static IEnumerable<string> GetDatabases(DomainConfiguration configuration)
    {
      return configuration.MappingRules
        .Select(r => r.Database)
        .Where(db => !string.IsNullOrEmpty(db))
        .Concat(Enumerable.Repeat(configuration.DefaultDatabase, 1))
        .Distinct();
    }

    private string FormatNodeName(string mappingDatabase, string mappingSchema, string mappingName)
    {
      return mappingDatabase + NameElementSeparator + mappingSchema + NameElementSeparator + mappingName;
    }

    // Constructors

    public MultidatabaseMappingResolver(DomainConfiguration configuration, NodeConfiguration nodeConfiguration)
    {
      databaseMapping = new NameMappingCollection();

      foreach (var database in configuration.Databases)
        if (!string.IsNullOrEmpty(database.RealName))
          databaseMapping.Add(database.Name, database.RealName);

      foreach (var item in nodeConfiguration.DatabaseMapping)
          databaseMapping.Add(item.Key, item.Value);

      schemaMapping = nodeConfiguration.SchemaMapping;

      defaultDatabase = configuration.DefaultDatabase;
      defaultSchema = configuration.DefaultSchema;

      var extractionTasksQuery =
        from db in GetDatabases(configuration)
        from schema in GetSchemasForDatabase(configuration, db)
        select new SqlExtractionTask(databaseMapping.Apply(db), schemaMapping.Apply(schema));
      extractionTasks = extractionTasksQuery.ToList();

      reversedDatabaseMapping = new NameMappingCollection();
      foreach (var mapping in databaseMapping)
        reversedDatabaseMapping.Add(mapping.Value, mapping.Key);

      reversedSchemaMapping = new NameMappingCollection();
      foreach (var mapping in schemaMapping)
        reversedSchemaMapping.Add(mapping.Value, mapping.Key);

      var metadataSchema = schemaMapping.Apply(defaultSchema);
      metadataTasks = extractionTasks.Where(t => t.Schema==metadataSchema).ToList();
    }
  }
}