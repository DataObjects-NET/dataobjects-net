// Copyright (C) 2012 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2012.02.10

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Xtensive.Diagnostics;
using Xtensive.Orm.Configuration;
using Xtensive.Reflection;

namespace Xtensive.Orm.Building.Builders
{
  internal sealed class StorageMappingBuilder
  {
    private struct MappingRequest : IEquatable<MappingRequest>
    {
      public readonly Assembly Assembly;
      public readonly string Namespace;

      #region Equality members

      public bool Equals(MappingRequest other)
      {
        return Assembly.Equals(other.Assembly) && string.Equals(Namespace, other.Namespace);
      }

      public override bool Equals(object obj)
      {
        if (ReferenceEquals(null, obj))
          return false;
        return obj is MappingRequest && Equals((MappingRequest) obj);
      }

      public override int GetHashCode()
      {
        unchecked {
          return (Assembly.GetHashCode() * 397) ^ Namespace.GetHashCode();
        }
      }

      public static bool operator ==(MappingRequest left, MappingRequest right)
      {
        return left.Equals(right);
      }

      public static bool operator !=(MappingRequest left, MappingRequest right)
      {
        return !left.Equals(right);
      }

      #endregion

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

    private readonly bool verbose;
    private readonly List<MappingRule> mappingRules;
    private readonly string defaultDatabase;
    private readonly string defaultSchema;

    public static void Run(BuildingContext context)
    {
      using (BuildLog.InfoRegion(Strings.LogProcessingMappingRules)) {
        new StorageMappingBuilder(context).ProcessAll();
      }
    }

    private void ProcessAll()
    {
      var typesToProcess = context.ModelDef.Types
        .Where(t => !t.IsGenericTypeDefinition && !t.IsAutoGenericInstance);

      foreach (var type in typesToProcess) {
        var underlyingType = type.UnderlyingType;
        if (verbose)
          BuildLog.Info(Strings.LogProcessingX, underlyingType.GetShortName());
        var request = new MappingRequest(underlyingType.Assembly, underlyingType.Namespace);
        MappingResult result;
        if (!mappingCache.TryGetValue(request, out result)) {
          result = Process(underlyingType);
          mappingCache.Add(request, result);
        }
        else {
          if (verbose)
            BuildLog.Info(Strings.LogReusingCachedMappingInformationForX, underlyingType.GetShortName());
        }
        type.MappingDatabase = result.MappingDatabase;
        type.MappingSchema = result.MappingSchema;
      }
    }

    private MappingResult Process(Type type)
    {
      var rule = mappingRules.First(r => RuleMatch(r, type));

      var resultDatabase = !string.IsNullOrEmpty(rule.Database) ? rule.Database : defaultDatabase;
      var resultSchema = !string.IsNullOrEmpty(rule.Schema) ? rule.Schema : defaultSchema;

      if (verbose)
        BuildLog.Info(Strings.ApplyingRuleXToY, rule, type.GetShortName());

      return new MappingResult(resultDatabase, resultSchema);
    }

    private static bool RuleMatch(MappingRule rule, Type type)
    {
      var assemblyMatch =
        rule.Assembly==null || rule.Assembly==type.Assembly;
      var namespaceMatch =
        string.IsNullOrEmpty(rule.Namespace) || type.FullName.StartsWith(rule.Namespace + ".");
      return assemblyMatch && namespaceMatch;
    }

    // Constructors

    private StorageMappingBuilder(BuildingContext context)
    {
      this.context = context;

      // Adding a special catch-all rule that maps all types to default schema/database.

      mappingRules = context.Configuration.MappingRules
        .Concat(Enumerable.Repeat(new MappingRule(null, null, null, null), 1))
        .ToList();

      defaultDatabase = context.Configuration.DefaultDatabase ?? string.Empty;
      defaultSchema = context.Configuration.DefaultSchema ?? string.Empty;

      verbose = BuildLog.IsLogged(LogEventTypes.Info);
    }
  }
}