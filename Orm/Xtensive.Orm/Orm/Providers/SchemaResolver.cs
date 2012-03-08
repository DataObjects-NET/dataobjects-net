// Copyright (C) 2012 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2012.03.06

using System.Collections.Generic;
using System.Linq;
using Xtensive.Orm.Configuration;
using Xtensive.Orm.Model;
using Xtensive.Sql;
using Xtensive.Sql.Model;

namespace Xtensive.Orm.Providers
{
  internal abstract class SchemaResolver
  {
    private sealed class SimpleSchemaResolver : SchemaResolver
    {
      private const string DummyName = "default";

      public override string GetSchemaName(string mappingDatabase, string mappingSchema)
      {
        return DummyName;
      }

      public override string GetSchemaName(SchemaNode node)
      {
        return DummyName;
      }

      public override Schema ResolveSchema(SqlExtractionResult model, string name)
      {
        return model.Catalogs.Single().Schemas.Single();
      }

      public override IEnumerable<string> GetAffectedSchemas(DomainModel model)
      {
        return Enumerable.Repeat(DummyName, 1);
      }

      public override IEnumerable<SqlExtractionTask> GetExtractionTasks(DomainModel model, ProviderInfo providerInfo)
      {
        var task = new SqlExtractionTask(providerInfo.DefaultDatabase, providerInfo.DefaultSchema);
        return Enumerable.Repeat(task, 1);
      }
    }

    private sealed class MultischemaModeSchemaResolver : SchemaResolver
    {
      private readonly string defaultMappingSchema;

      public override string GetSchemaName(string mappingDatabase, string mappingSchema)
      {
        return string.IsNullOrEmpty(mappingSchema) ? defaultMappingSchema : mappingSchema;
      }

      public override string GetSchemaName(SchemaNode node)
      {
        return node.Schema.Name;
      }

      public override Schema ResolveSchema(SqlExtractionResult model, string name)
      {
        return model.Catalogs.Single().Schemas[name];
      }

      public override IEnumerable<string> GetAffectedSchemas(DomainModel model)
      {
        var schemas = GetSchemasQuery(model);
        return schemas.ToList();
      }

      public override IEnumerable<SqlExtractionTask> GetExtractionTasks(DomainModel model, ProviderInfo providerInfo)
      {
        var tasks = GetSchemasQuery(model)
          .Select(s => new SqlExtractionTask(providerInfo.DefaultDatabase, s))
          .ToList();
        return tasks;
      }

      private IEnumerable<string> GetSchemasQuery(DomainModel model)
      {
        return model.Types
          .Select(t => t.MappingSchema)
          .Where(s => !string.IsNullOrEmpty(s))
          .Concat(Enumerable.Repeat(defaultMappingSchema, 1))
          .Distinct();
      }

      // Constructors

      public MultischemaModeSchemaResolver(string defaultMappingSchema)
      {
        this.defaultMappingSchema = defaultMappingSchema;
      }
    }

    private sealed class MultidatabaseModeSchemaResolver : SchemaResolver
    {
      private const char Separator = ':'; // Colon is forbidden by name validator

      private readonly Dictionary<string, string> aliasMap;
      private readonly string defaultMappingDatabase;
      private readonly string defaultMappingSchema;

      public override string GetSchemaName(string mappingDatabase, string mappingSchema)
      {
        // The only reason this method accepts empty database or schema is HintGenerator.
        // It needs resolving mapping information from previous upgrade
        // which could be performed in different multidatabase/multischema mode.
        // If such case occurs we simply use current domain defaults as mapping information.

        if (string.IsNullOrEmpty(mappingDatabase))
          mappingDatabase = defaultMappingDatabase;
        
        if (string.IsNullOrEmpty(mappingSchema))
          mappingSchema = defaultMappingSchema;

        return FormatSchemaName(ResolveAlias(mappingDatabase), mappingSchema);
      }

      public override string GetSchemaName(SchemaNode node)
      {
        var schema = node.Schema;
        return FormatSchemaName(schema.Catalog.Name, schema.Name);
      }

      public override Schema ResolveSchema(SqlExtractionResult model, string name)
      {
        var names = name.Split(Separator);
        return model.Catalogs[names[0]].Schemas[names[1]];
      }

      public override IEnumerable<string> GetAffectedSchemas(DomainModel model)
      {
        return GetDatabases(model)
          .SelectMany(db => GetSchemasForDatabase(model, db), GetSchemaName)
          .ToList();
      }

      public override IEnumerable<SqlExtractionTask> GetExtractionTasks(DomainModel model, ProviderInfo providerInfo)
      {
        return GetDatabases(model)
          .SelectMany(db => GetSchemasForDatabase(model, db),
            (db, schema) => new SqlExtractionTask(ResolveAlias(db), schema))
          .ToList();
      }

      private IEnumerable<string> GetDatabases(DomainModel model)
      {
        return model.Types
          .Select(t => t.MappingDatabase)
          .Where(db => !string.IsNullOrEmpty(db))
          .Concat(Enumerable.Repeat(defaultMappingDatabase, 1))
          .Distinct();
      }

      private IEnumerable<string> GetSchemasForDatabase(DomainModel model, string database)
      {
        return model.Types
          .Where(t => t.MappingDatabase==database)
          .Select(t => t.MappingSchema)
          .Concat(Enumerable.Repeat(defaultMappingSchema, 1))
          .Distinct();
      }

      private string ResolveAlias(string alias)
      {
        string result;
        return aliasMap.TryGetValue(alias, out result) ? result : alias;
      }
    
      private string FormatSchemaName(string mappingDatabase, string mappingSchema)
      {
        return string.Format("{0}{1}{2}", mappingDatabase, Separator, mappingSchema);
      }

      // Constructors

      public MultidatabaseModeSchemaResolver(DomainConfiguration configuration)
      {
        aliasMap = configuration.DatabaseAliases.ToDictionary(alias => alias.Name, alias => alias.Database);
        defaultMappingDatabase = configuration.DefaultDatabase;
        defaultMappingSchema = configuration.DefaultSchema;
      }
    }

    public string GetSchemaName(SchemaMappedNode node) // Domain model
    {
      return GetSchemaName(node.MappingDatabase, node.MappingSchema);
    }

    public abstract string GetSchemaName(string mappingDatabase, string mappingSchema); // Custom mapping information

    public abstract string GetSchemaName(SchemaNode node); // SQL model

    public abstract Schema ResolveSchema(SqlExtractionResult model, string name);

    public abstract IEnumerable<string> GetAffectedSchemas(DomainModel model);

    public abstract IEnumerable<SqlExtractionTask> GetExtractionTasks(DomainModel model, ProviderInfo providerInfo);

    public static SchemaResolver Get(DomainConfiguration configuration)
    {
      if (configuration.IsMultidatabase)
        return new MultidatabaseModeSchemaResolver(configuration);
      if (configuration.IsMultischema)
        return new MultischemaModeSchemaResolver(configuration.DefaultSchema);
      return new SimpleSchemaResolver();
    }
  }
}