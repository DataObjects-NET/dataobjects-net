// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Kofman
// Created:    2009.04.14

using System;
using System.Linq;
using Xtensive.Storage.Model;

namespace Xtensive.Storage.Building.Builders
{  
  internal static class TypeIdBuilder
  {
    public static void BuildTypeIds()
    {
      var context = BuildingContext.Demand();
      BuildSystemTypeIds(context);
      BuildRegularTypeIds(context);
      BuildInterfaceAndStructureTypeIds(context);
      context.Model.Types.BuildTypeIdIndex();
    }

    private static void BuildSystemTypeIds(BuildingContext context)
    {
      foreach (var type in context.SystemTypeIds.Keys) {
        var typeInfo = context.Model.Types[type];
        typeInfo.TypeId = context.SystemTypeIds[type];
      }
    }

    private static void BuildRegularTypeIds(BuildingContext context)
    {
      var typeIdProvider = context.BuilderConfiguration.TypeIdProvider
        ?? (type => TypeInfo.NoTypeId);
      var typesToProcess = context.Model.Types
        .Where(type => type.IsEntity && type.TypeId==TypeInfo.NoTypeId)
        .ToArray();
      var providedIds = typesToProcess
        .Select(type => new {Type = type, Id = typeIdProvider.Invoke(type.UnderlyingType)})
        .Where(item => item.Id != TypeInfo.NoTypeId)
        .ToArray();
      int nextTypeId = providedIds
        .Select(item => item.Id)
        .DefaultIfEmpty(TypeInfo.MinTypeId)
        .Max() + 1;
      foreach (var item in providedIds)
        item.Type.TypeId = item.Id;
      foreach (var type in typesToProcess.Except(providedIds.Select(item => item.Type)))
        type.TypeId = nextTypeId++;
    }

    private static void BuildInterfaceAndStructureTypeIds(BuildingContext context)
    {
      int nextTypeId = context.Model.Types
        .Where(type => type.IsEntity && type.TypeId!=TypeInfo.NoTypeId)
        .Select(type => type.TypeId)
        .AddOne(TypeInfo.MinTypeId) // .Max() fails if there are no items
        .Max() + 1;
      var typesToProcess = context.Model.Types
        .Where(type => (type.IsStructure || type.IsInterface) && type.TypeId==TypeInfo.NoTypeId);
      foreach (var type in typesToProcess)
        type.TypeId = nextTypeId++;
    }
  }
}