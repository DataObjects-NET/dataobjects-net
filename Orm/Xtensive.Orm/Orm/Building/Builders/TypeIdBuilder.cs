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

    public void BuildTypeIds()
    {
      AssignTypeIds();

      // Updating TypeId index
      domain.Model.Types.RebuildTypeIdIndex();
    }

    private void AssignTypeIds()
    {
      // Load existing type ids
      foreach (var type in GetTypesWithoutTypeId())
        type.TypeId = typeIdProvider.GetTypeId(type.UnderlyingType);

      // Provide type ids for remaining types
      var typeGroups = GetTypesWithoutTypeId()
        .GroupBy(t => t.MappingDatabase ?? string.Empty)
        .OrderBy(g => g.Key);
      foreach (var group in typeGroups) {
        var sequence = GetTypeIdSequence(group.Key);
        foreach (var type in group.OrderBy(i => i.Name))
          type.TypeId = sequence.GetNextValue();
      }
    }

    private TypeIdSequence GetTypeIdSequence(string mappingDatabase)
    {
      if (domain.Model.Databases.Count==0)
        // Multidatabase mode is not enabled, use default range
        return GetTypeIdSequence(TypeInfo.MinTypeId, int.MaxValue, Strings.NA);

      // Query configuration for a particular database
      var configurationEntry = domain.Model.Databases[mappingDatabase].Configuration;
      return GetTypeIdSequence(
        configurationEntry.MinTypeId, configurationEntry.MaxTypeId, configurationEntry.Name);
    }

    private TypeIdSequence GetTypeIdSequence(int minTypeId, int maxTypeId, string mappingDatabase)
    {
      var current = domain.Model.Types
        .Where(t => t.TypeId >= minTypeId && t.TypeId <= maxTypeId)
        .Select(t => t.TypeId)
        .DefaultIfEmpty(minTypeId - 1)
        .Max();
      return new TypeIdSequence(current, minTypeId, maxTypeId, mappingDatabase);
    }

    private IEnumerable<TypeInfo> GetTypesWithoutTypeId()
    {
      return domain.Model.Types.Where(type => type.IsEntity && type.TypeId==TypeInfo.NoTypeId);
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