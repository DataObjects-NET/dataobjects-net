// Copyright (C) 2012 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2012.03.06

using System;
using System.Collections.Generic;
using System.Linq;
using Xtensive.Core;
using Xtensive.Orm.Configuration;
using Xtensive.Orm.Model;
using Xtensive.Sql;
using Xtensive.Sql.Model;

namespace Xtensive.Orm.Providers
{
  internal abstract class MappingResolver
  {
    private const char Separator = ':'; // This char is forbidden by name validator

    private sealed class SimpleResolver : MappingResolver
    {
      private readonly ProviderInfo providerInfo;

      public override string GetNodeName(string mappingDatabase, string mappingSchema, string mappingName)
      {
        return mappingName;
      }

      public override string GetNodeName(SchemaNode node)
      {
        return node.Name;
      }

      public override MappingResolveResult Resolve(SqlExtractionResult model, string nodeName)
      {
        var schema = model.Catalogs.Single().Schemas.Single();
        return new MappingResolveResult(schema, nodeName);
      }

      public override IEnumerable<SqlExtractionTask> GetExtractionTasks()
      {
        var task = new SqlExtractionTask(providerInfo.DefaultDatabase, providerInfo.DefaultSchema);
        return Enumerable.Repeat(task, 1);
      }

      // Constructors

      public SimpleResolver(ProviderInfo providerInfo)
      {
        this.providerInfo = providerInfo;
      }
    }

    private sealed class MultischemaResolver : MappingResolver
    {
      private readonly ProviderInfo providerInfo;
      private readonly string defaultSchema;
      private readonly List<string> allSchemas;

      public override string GetNodeName(string mappingDatabase, string mappingSchema, string mappingName)
      {
        if (string.IsNullOrEmpty(mappingSchema))
          mappingSchema = defaultSchema;
        return FormatNodeName(mappingSchema, mappingName);
      }

      public override string GetNodeName(SchemaNode node)
      {
        return FormatNodeName(node.Schema.Name, node.Name);
      }

      public override MappingResolveResult Resolve(SqlExtractionResult model, string nodeName)
      {
        var names = nodeName.Split(Separator);
        var schema = model.Catalogs.Single().Schemas[names[0]];
        var name = names[1];
        return new MappingResolveResult(schema, name);
      }

      public override IEnumerable<SqlExtractionTask> GetExtractionTasks()
      {
        return allSchemas.Select(s => new SqlExtractionTask(providerInfo.DefaultDatabase, s));
      }

      private string FormatNodeName(string mappingSchema, string mappingName)
      {
        return mappingSchema + Separator + mappingName;
      }


      // Constructors

      public MultischemaResolver(DomainConfiguration configuration, ProviderInfo providerInfo)
      {
        this.providerInfo = providerInfo;
        defaultSchema = configuration.DefaultSchema;
        allSchemas = configuration.MappingRules
          .Select(r => r.Schema)
          .Where(s => !string.IsNullOrEmpty(s))
          .Concat(Enumerable.Repeat(configuration.DefaultSchema, 1))
          .Distinct()
          .ToList();
      }
    }

    private sealed class MultidatabaseResolver : MappingResolver
    {
      private readonly string defaultDatabase;
      private readonly string defaultSchema;

      private readonly Dictionary<string, string> aliasMap;
      private readonly List<Pair<string>> allSchemas;

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

        return FormatNodeName(ResolveAlias(mappingDatabase), mappingSchema, mappingName);
      }

      public override string GetNodeName(SchemaNode node)
      {
        var schema = node.Schema;
        return FormatNodeName(schema.Catalog.Name, schema.Name, node.Name);
      }

      public override MappingResolveResult Resolve(SqlExtractionResult model, string nodeName)
      {
        var names = nodeName.Split(Separator);
        var schema = model.Catalogs[names[0]].Schemas[names[1]];
        var name = names[2];
        return new MappingResolveResult(schema, name);
      }

      public override IEnumerable<SqlExtractionTask> GetExtractionTasks()
      {
        return allSchemas.Select(item => new SqlExtractionTask(item.First, item.Second));
      }

      private static IEnumerable<string> GetSchemasForDatabase(DomainConfiguration configuration, string database)
      {
        var userSchemas =
          from rule in configuration.MappingRules
          let db = String.IsNullOrEmpty(rule.Database) ? configuration.DefaultDatabase : rule.Database
          where db==database
          select String.IsNullOrEmpty(rule.Schema) ? configuration.DefaultSchema : rule.Schema;

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
          .Distinct()
          .ToList();
      }

      private string ResolveAlias(string alias)
      {
        string name;
        return aliasMap.TryGetValue(alias, out name) ? name : alias;
      }

      private string FormatNodeName(string mappingDatabase, string mappingSchema, string mappingName)
      {
        return mappingDatabase + Separator + mappingSchema + Separator + mappingName;
      }

      // Constructors

      public MultidatabaseResolver(DomainConfiguration configuration)
      {
        aliasMap = configuration.DatabaseAliases
          .ToDictionary(alias => alias.Name, alias => alias.Database);

        defaultDatabase = configuration.DefaultDatabase;
        defaultSchema = configuration.DefaultSchema;

        var allSchemaQuery =
          from db in GetDatabases(configuration)
          from schema in GetSchemasForDatabase(configuration, db)
          select new Pair<string>(ResolveAlias(db), schema);

        allSchemas = allSchemaQuery.ToList();
      }
    }

    public string GetNodeName(SchemaMappedNode node)
    {
      return GetNodeName(node.MappingDatabase, node.MappingSchema, node.MappingName);
    }

    public abstract string GetNodeName(string mappingDatabase, string mappingSchema, string mappingName);

    public abstract string GetNodeName(SchemaNode node);

    public abstract MappingResolveResult Resolve(SqlExtractionResult model, string nodeName);

    public abstract IEnumerable<SqlExtractionTask> GetExtractionTasks();

    public static MappingResolver Get(DomainConfiguration configuration, ProviderInfo providerInfo)
    {
      if (configuration.IsMultidatabase)
        return new MultidatabaseResolver(configuration);
      if (configuration.IsMultischema)
        return new MultischemaResolver(configuration, providerInfo);
      return new SimpleResolver(providerInfo);
    }
  }
}