// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Kofman
// Created:    2009.04.14

using System;
using System.Collections.Generic;
using System.Linq;
using Xtensive.Core;
using Xtensive.Orm.Internals;
using Xtensive.Orm.Model;

namespace Xtensive.Orm.Building.Builders
{  
  internal sealed class TypeIdBuilder
  {
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
        var nextTypeId = GetMaximalTypeId(group.Key) + 1;
        foreach (var type in group.OrderBy(i => i.Name))
          type.TypeId = nextTypeId++;
      }
    }

    private int GetMaximalTypeId(string mappingDatabase)
    {
      if (domain.Model.Databases.Count==0)
        // Multidatabase mode is not enabled, use default range
        return GetMaximalTypeId(TypeInfo.MinTypeId, int.MaxValue, Strings.NA);

      // Query configuration for a particular database
      var configurationEntry = domain.Model.Databases[mappingDatabase].Configuration;
      return GetMaximalTypeId(
        configurationEntry.MinTypeId, configurationEntry.MaxTypeId, configurationEntry.Name);
    }

    private int GetMaximalTypeId(int minTypeId, int maxTypeId, string mappingDatabase)
    {
      var result = domain.Model.Types
        .Where(t => t.TypeId >= minTypeId && t.TypeId <= maxTypeId)
        .Select(t => t.TypeId)
        .DefaultIfEmpty(minTypeId)
        .Max();
      if (result==maxTypeId)
        throw new InvalidOperationException(string.Format(
          Strings.TypeIdRangeForDatabaseXYZIsExhausted, mappingDatabase, minTypeId, maxTypeId));
      return result;
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