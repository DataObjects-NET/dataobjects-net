// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Kofman
// Created:    2009.04.14

using System;
using System.Collections.Generic;
using System.Linq;
using Xtensive.Core;
using Xtensive.Orm.Model;

namespace Xtensive.Orm.Building.Builders
{  
  internal sealed class TypeIdBuilder
  {
    private sealed class TypeIdSequence
    {
      private int currentValue;
      private readonly int minValue;
      private readonly int maxValue;
      private readonly string mappingDatabase;

      public int GetNextValue()
      {
        if (currentValue==maxValue)
          throw new InvalidOperationException(string.Format(
            Strings.TypeIdRangeForDatabaseXYZIsExhausted, mappingDatabase, minValue, maxValue));
        return ++currentValue;
      }

      public TypeIdSequence(int currentValue, int minValue, int maxValue, string mappingDatabase)
      {
        this.currentValue = currentValue;
        this.minValue = minValue;
        this.maxValue = maxValue;
        this.mappingDatabase = mappingDatabase;
      }
    }

    private readonly Domain domain;
    private readonly ITypeIdProvider typeIdProvider;

    public void BuildTypeIds(TypeIdRegistry registry)
    {
      // Import static type identifiers
      foreach (var type in GetTypesWithTypeId())
        registry.Register(type.TypeId, type);

      // Load old type identifiers from database
      foreach (var type in GetTypesWithoutTypeId(registry)) {
        var typeId = typeIdProvider.GetTypeId(type.UnderlyingType);
        if (typeId!=TypeInfo.NoTypeId)
          registry.Register(typeId, type);
      }

      // Generate type identifiers for remaining types
      var typeGroups = GetTypesWithoutTypeId(registry)
        .GroupBy(t => t.MappingDatabase ?? string.Empty)
        .OrderBy(g => g.Key);

      foreach (var group in typeGroups) {
        var sequence = GetTypeIdSequence(registry, group.Key);
        foreach (var type in group.OrderBy(i => i.Name))
          registry.Register(sequence.GetNextValue(), type);
      }
    }

    public void SetDefaultTypeIds(TypeIdRegistry registry)
    {
      foreach (var type in GetTypesWithoutTypeId())
        type.TypeId = registry[type];
      domain.Model.Types.TypeIdRegistry = registry;
    }

    private TypeIdSequence GetTypeIdSequence(TypeIdRegistry registry, string mappingDatabase)
    {
      if (domain.Model.Databases.Count==0)
        // Multidatabase mode is not enabled, use default range
        return GetTypeIdSequence(registry, TypeInfo.MinTypeId, int.MaxValue, Strings.NA);

      // Query configuration for a particular database
      var configurationEntry = domain.Model.Databases[mappingDatabase].Configuration;
      return GetTypeIdSequence(registry,
        configurationEntry.MinTypeId, configurationEntry.MaxTypeId, configurationEntry.Name);
    }

    private TypeIdSequence GetTypeIdSequence(TypeIdRegistry registry, int minTypeId, int maxTypeId, string mappingDatabase)
    {
      var current = registry.TypeIdentifiers
        .Where(id => id >= minTypeId && id <= maxTypeId)
        .DefaultIfEmpty(minTypeId - 1)
        .Max();
      return new TypeIdSequence(current, minTypeId, maxTypeId, mappingDatabase);
    }

    private IEnumerable<TypeInfo> GetTypesWithTypeId()
    {
      return domain.Model.Types.Where(t => t.IsEntity && t.TypeId!=TypeInfo.NoTypeId);
    }

    private IEnumerable<TypeInfo> GetTypesWithoutTypeId()
    {
      return domain.Model.Types.Where(t => t.IsEntity && t.TypeId==TypeInfo.NoTypeId);
    }

    private IEnumerable<TypeInfo> GetTypesWithoutTypeId(TypeIdRegistry registry)
    {
      return domain.Model.Types.Where(t => t.IsEntity && !registry.Contains(t));
    }

    // Constructors

    public TypeIdBuilder(Domain domain, ITypeIdProvider typeIdProvider)
    {
      ArgumentValidator.EnsureArgumentNotNull(domain, "domain");
      ArgumentValidator.EnsureArgumentNotNull(typeIdProvider, "typeIdProvider");

      this.domain = domain;
      this.typeIdProvider = typeIdProvider;
    }
  }
}