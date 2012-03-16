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
  internal static class TypeIdBuilder
  {
    public static void BuildTypeIds(bool systemTypesOnly)
    {
      var context = BuildingContext.Demand();
      var domain = context.Domain;
      
      BuildSystemTypeIds(context);
      if (!systemTypesOnly)
        BuildRegularTypeIds(context);

      // Updating TypeId index
      context.Model.Types.RebuildTypeIdIndex();
      // Updating Type-leve caches
      var typeLevelCaches = domain.TypeLevelCaches;
      foreach (var type in domain.Model.Types) {
        int typeId = type.TypeId;
        if (typeId!=TypeInfo.NoTypeId) {
          TypeLevelCache cache;
          if (!typeLevelCaches.TryGetValue(typeId, out cache))
            typeLevelCaches.Add(typeId, new TypeLevelCache(type));
        }
      }
    }

    private static void BuildSystemTypeIds(BuildingContext context)
    {
      foreach (var type in context.SystemTypeIds.Keys) {
        var typeInfo = context.Model.Types[type];
        int systemTypeId = context.SystemTypeIds[type];
// ReSharper disable RedundantCheckBeforeAssignment
        if (typeInfo.TypeId!=systemTypeId)
          typeInfo.TypeId = systemTypeId;
// ReSharper restore RedundantCheckBeforeAssignment
      }
    }

    private static void BuildRegularTypeIds(BuildingContext context)
    {
      var getTypeId = context.BuilderConfiguration.TypeIdProvider ?? (type => TypeInfo.NoTypeId);
      var maxTypeId = context.Model.Types
        .Where(type => type.TypeId >= TypeInfo.MinTypeId)
        .Select(type => type.TypeId)
        .DefaultIfEmpty(TypeInfo.MinTypeId)
        .Max();
      var typesToProcess = context.Model.Types
        .Where(type => type.TypeId == TypeInfo.NoTypeId && type.UnderlyingType != typeof(Structure))
        .Select(type => new {Type = type, Id = type.IsEntity 
          ? getTypeId(type.UnderlyingType) 
          : TypeInfo.NoTypeId })
        .OrderByDescending(x => x.Type.IsEntity)
        .ThenBy(x => x.Type.Name)
        .ToList();
      var nextTypeId = typesToProcess
        .Where(x => x.Id != TypeInfo.NoTypeId)
        .Select(x => x.Id)
        .AddOne(maxTypeId)
        .Max() + 1;
      foreach (var pair in typesToProcess)
        pair.Type.TypeId = pair.Id == TypeInfo.NoTypeId
          ? nextTypeId++
          : pair.Id;
    }
  }
}