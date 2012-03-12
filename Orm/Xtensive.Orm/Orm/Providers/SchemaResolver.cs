// Copyright (C) 2012 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2012.03.06

using System.Collections.Generic;
using System.Linq;
using Xtensive.Core;
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

      public override IEnumerable<string> GetAllSchemas()
      {
        return Enumerable.Repeat(DummyName, 1);
      }

      public override IEnumerable<SqlExtractionTask> GetExtractionTasks(ProviderInfo providerInfo)
      {
        var task = new SqlExtractionTask(providerInfo.DefaultDatabase, providerInfo.DefaultSchema);
        return Enumerable.Repeat(task, 1);
      }
    }

    private sealed class MultischemaModeSchemaResolver : SchemaResolver
    {
      private readonly string defaultSchema;
      private readonly List<string> allSchemas;

      public override string GetSchemaName(string mappingDatabase, string mappingSchema)
      {
        return string.IsNullOrEmpty(mappingSchema) ? defaultSchema : mappingSchema;
      }

      public override string GetSchemaName(SchemaNode node)
      {
        return node.Schema.Name;
      }

      public override Schema ResolveSchema(SqlExtractionResult model, string name)
      {
        return model.Catalogs.Single().Schemas[name];
      }

      public override IEnumerable<string> GetAllSchemas()
      {
        return allSchemas;
      }

      public override IEnumerable<SqlExtractionTask> GetExtractionTasks(ProviderInfo providerInfo)
      {
        return allSchemas.Select(s => new SqlExtractionTask(providerInfo.DefaultDatabase, s));
      }


      // Constructors

      public MultischemaModeSchemaResolver(DomainConfiguration configuration)
      {
        defaultSchema = configuration.DefaultSchema;
        allSchemas = configuration.MappingRules
          .Select(r => r.Schema)
          .Where(s => !string.IsNullOrEmpty(s))
          .Concat(Enumerable.Repeat(configuration.DefaultSchema, 1))
          .Distinct()
          .ToList();
      }
    }

    private sealed class MultidatabaseModeSchemaResolver : SchemaResolver
    {
      private const char Separator = ':'; // This char is forbidden by name validator

      private readonly string defaultDatabase;
      private readonly string defaultSchema;

      private readonly Dictionary<string, string> aliasMap;
      private readonly List<Pair<string>> allSchemas;

      public override string GetSchemaName(string mappingDatabase, string mappingSchema)
      {
        // The only reason this method accepts empty database or schema is HintGenerator.
        // It needs resolving mapping information from previous upgrade
        // which could be performed in different multidatabase/multischema mode.
        // If such case occurs we simply use current domain defaults as mapping information.

        if (string.IsNullOrEmpty(mappingDatabase))
          mappingDatabase = defaultDatabase;
        
        if (string.IsNullOrEmpty(mappingSchema))
          mappingSchema = defaultSchema;

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

      public override IEnumerable<string> GetAllSchemas()
      {
        return allSchemas.Select(item => FormatSchemaName(item.First, item.Second));
      }

      public override IEnumerable<SqlExtractionTask> GetExtractionTasks(ProviderInfo providerInfo)
      {
        return allSchemas.Select(item => new SqlExtractionTask(item.First, item.Second));
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

      private string ResolveAlias(string alias)
      {
        string name;
        return aliasMap.TryGetValue(alias, out name) ? name : alias;
      }

      private string FormatSchemaName(string mappingDatabase, string mappingSchema)
      {
        return string.Format("{0}{1}{2}", mappingDatabase, Separator, mappingSchema);
      }

      // Constructors

      public MultidatabaseModeSchemaResolver(DomainConfiguration configuration)
      {
        aliasMap = configuration.DatabaseAliases
          .ToDictionary(alias => alias.Name, alias => alias.Database);

        defaultDatabase = configuration.DefaultDatabase;
        defaultSchema = configuration.DefaultSchema;

        var allSchemaQuery =
          from db in configuration.GetDatabases()
          from schema in GetSchemasForDatabase(configuration, db)
          select new Pair<string>(ResolveAlias(db), schema);

        allSchemas = allSchemaQuery.ToList();
      }
    }

    public string GetSchemaName(SchemaMappedNode node) // Domain model
    {
      return GetSchemaName(node.MappingDatabase, node.MappingSchema);
    }

    public abstract string GetSchemaName(string mappingDatabase, string mappingSchema); // Custom mapping information

    public abstract string GetSchemaName(SchemaNode node); // SQL model

    public abstract Schema ResolveSchema(SqlExtractionResult model, string name);

    public abstract IEnumerable<string> GetAllSchemas();

    public abstract IEnumerable<SqlExtractionTask> GetExtractionTasks(ProviderInfo providerInfo);

    public static SchemaResolver Get(DomainConfiguration configuration)
    {
      if (configuration.IsMultidatabase)
        return new MultidatabaseModeSchemaResolver(configuration);
      if (configuration.IsMultischema)
        return new MultischemaModeSchemaResolver(configuration);
      return new SimpleSchemaResolver();
    }
  }
}