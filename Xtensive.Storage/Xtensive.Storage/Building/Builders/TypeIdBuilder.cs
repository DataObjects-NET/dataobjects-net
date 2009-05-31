// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Kofman
// Created:    2009.04.14

using System.Linq;
using Xtensive.Storage.Model;

namespace Xtensive.Storage.Building.Builders
{  
  internal static class TypeIdBuilder
  {
    public static void BuildTypeIds()
    {
      BuildSystemTypeIds();
      BuildRegularTypeIds();
    }

    public static void BuildSystemTypeIds()
    {
      var context = BuildingContext.Demand();
      foreach (var type in context.SystemTypeIds.Keys) {
        var typeInfo = context.Model.Types[type];
        AssignTypeId(typeInfo, context.SystemTypeIds[type]);
      }
      context.Model.Types.BuildTypeIdIndex();
    }

    public static void BuildRegularTypeIds()
    {
      var context = BuildingContext.Demand();
      var typeIdProvider = context.BuilderConfiguration.TypeIdProvider
        ?? (type => TypeInfo.NoTypeId);
      var typesToProcess = context.Model.Types
        .Where(type => type.IsEntity && type.TypeId==TypeInfo.NoTypeId)
        .ToArray();
      var providedIds = typesToProcess
        .Select(type => new {Type = type, Id = typeIdProvider.Invoke(type.UnderlyingType)})
        .Where(item => item.Id!=TypeInfo.NoTypeId)
        .ToArray();
      int firstId = providedIds
        .Select(item => item.Id)
        .DefaultIfEmpty(TypeInfo.MinTypeId)
        .Max();
      firstId++;
      foreach (var item in providedIds)
        AssignTypeId(item.Type, item.Id);
      foreach (var type in typesToProcess.Except(providedIds.Select(item => item.Type)))
        AssignTypeId(type, firstId++);
      context.Model.Types.BuildTypeIdIndex();
    }

    private static void AssignTypeId(TypeInfo type, int typeId)
    {
      type.SetTypeId(typeId, BuildingContext.Current.ModelUnlockKey);
    }
  }
}