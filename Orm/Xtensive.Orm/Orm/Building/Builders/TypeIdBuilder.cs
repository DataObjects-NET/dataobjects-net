// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Kofman
// Created:    2009.04.14

using System;
using System.Linq;
using Xtensive.Core;
using Xtensive.Orm.Internals;
using Xtensive.Orm.Model;

namespace Xtensive.Orm.Building.Builders
{  
  internal sealed class TypeIdBuilder
  {
    private readonly Domain domain;
    private readonly Func<Type, int> typeIdProvider;

    public void BuildTypeIds()
    {
      BuildRegularTypeIds();

      // Updating TypeId index
      domain.Model.Types.RebuildTypeIdIndex();

      // Updating type-level caches
      RebuildTypeLevelCaches();
    }

    private void RebuildTypeLevelCaches()
    {
      var caches = domain.TypeLevelCaches;
      foreach (var type in domain.Model.Types) {
        var typeId = type.TypeId;
        if (typeId==TypeInfo.NoTypeId)
          continue;
        TypeLevelCache cache;
        if (!caches.TryGetValue(typeId, out cache))
          caches.Add(typeId, new TypeLevelCache(type));
      }
    }

    private void BuildRegularTypeIds()
    {
      var maxTypeId = domain.Model.Types
        .Where(type => type.TypeId >= TypeInfo.MinTypeId)
        .Select(type => type.TypeId)
        .DefaultIfEmpty(TypeInfo.MinTypeId)
        .Max();
      var typesToProcess = domain.Model.Types
        .Where(type => type.TypeId==TypeInfo.NoTypeId && type.UnderlyingType!=typeof (Structure))
        .Select(type => new {
          Type = type,
          Id = type.IsEntity ? typeIdProvider.Invoke(type.UnderlyingType) : TypeInfo.NoTypeId
        })
        .OrderByDescending(x => x.Type.IsEntity)
        .ThenBy(x => x.Type.Name)
        .ToList();
      var nextTypeId = typesToProcess
        .Where(x => x.Id!=TypeInfo.NoTypeId)
        .Select(x => x.Id)
        .AddOne(maxTypeId)
        .Max() + 1;
      foreach (var pair in typesToProcess)
        pair.Type.TypeId = pair.Id==TypeInfo.NoTypeId
          ? nextTypeId++
          : pair.Id;
    }

    // Constructors

    public TypeIdBuilder(Domain domain, Func<Type, int> typeIdProvider)
    {
      ArgumentValidator.EnsureArgumentNotNull(domain, "domain");
      ArgumentValidator.EnsureArgumentNotNull(typeIdProvider, "typeIdProvider");

      this.domain = domain;
      this.typeIdProvider = typeIdProvider;
    }
  }
}