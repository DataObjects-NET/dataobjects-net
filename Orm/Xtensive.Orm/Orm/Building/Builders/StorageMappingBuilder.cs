// Copyright (C) 2012 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2012.02.10

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Xtensive.Orm.Building.Builders
{
  internal sealed class StorageMappingBuilder
  {
    private struct MappingRequest
    {
      public readonly Assembly Assembly;
      public readonly string Namespace;

      public MappingRequest(Assembly assembly, string @namespace)
      {
        Assembly = assembly;
        Namespace = @namespace;
      }
    }

    private struct MappingResult
    {
      public readonly string MappingDatabase;
      public readonly string MappingSchema;

      public MappingResult(string mappingDatabase, string mappingSchema)
      {
        MappingDatabase = mappingDatabase;
        MappingSchema = mappingSchema;
      }
    }

    private readonly BuildingContext context;
    private readonly Dictionary<MappingRequest, MappingResult> mappingCache
      = new Dictionary<MappingRequest,MappingResult>();

    public static void Run(BuildingContext context)
    {
      new StorageMappingBuilder(context).ProcessAll();
    }

    private void ProcessAll()
    {
      var typesToProcess = context.ModelDef.Types
        .Where(t => t.IsEntity);
      foreach (var type in typesToProcess) {
        var underlyingType = type.UnderlyingType;
        var request = new MappingRequest(underlyingType.Assembly, underlyingType.Namespace);
        MappingResult result;
        if (!mappingCache.TryGetValue(request, out result)) {
          result = Process(underlyingType);
          mappingCache.Add(request, result);
        }
        type.MappingDatabase = result.MappingDatabase;
        type.MappingSchema = result.MappingSchema;
      }
    }

    private MappingResult Process(Type type)
    {
      var configuration = context.Configuration;
      var targetDatabase = configuration.DefaultDatabase;
      var targetSchema = configuration.DefaultSchema;

      foreach (var rule in configuration.MappingRules) {
        var assemblyMatch = rule.Assembly==null
          || rule.Assembly==type.Assembly;
        var namespaceMatch = string.IsNullOrEmpty(rule.Namespace)
          || type.FullName.StartsWith(rule.Namespace + ".");

        if (assemblyMatch && namespaceMatch) {
          if (!string.IsNullOrEmpty(rule.Database))
            targetDatabase = rule.Database;
          if (!string.IsNullOrEmpty(rule.Schema))
            targetSchema = rule.Schema;
        }
      }

      return new MappingResult(targetDatabase, targetSchema);
    }

    // Constructors

    private StorageMappingBuilder(BuildingContext context)
    {
      this.context = context;
    }
  }
}