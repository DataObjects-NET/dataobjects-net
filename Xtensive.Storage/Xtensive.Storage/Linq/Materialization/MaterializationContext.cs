// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexis Kochetov
// Created:    2009.05.29

using System;
using System.Collections.Generic;
using System.Linq;
using Xtensive.Core;
using Xtensive.Core.Tuples.Transform;
using Xtensive.Storage.Internals;
using Xtensive.Storage.Model;

namespace Xtensive.Storage.Linq.Materialization
{
  internal sealed class MaterializationContext
  {
    #region Nested type: EntityMappingCache

    private struct EntityMappingCache
    {
      public TypeMapping SingleItem;
      public Dictionary<int, TypeMapping> Items;
    }

    #endregion
    
    private readonly EntityMappingCache[] entityMappings;
    private readonly DomainModel model;
    public int EntitiesInRow;
    public Queue<Action> MaterializationQueue;

    public TypeMapping GetTypeMapping(int entityIndex, TypeInfo approximateType, int typeId, Pair<int>[] columns)
    {
      TypeMapping result;
      var cache = entityMappings[entityIndex];
      if (cache.SingleItem!=null) {
        if (typeId!=cache.SingleItem.TypeId)
          throw new ArgumentOutOfRangeException("typeId");
        return cache.SingleItem;
      }
      if (cache.Items.TryGetValue(typeId, out result))
        return result;

      var type       = model.Types[typeId];
      var keyInfo    = type.Key;
      var descriptor = type.TupleDescriptor;

      var typeColumnMap = columns.ToArray();
      if (approximateType.IsInterface)
        // fixup target index
        for (int i = 0; i < columns.Length; i++) {
          var pair = typeColumnMap[i];
          var approxTargetIndex = pair.First;
          var interfaceField = approximateType.Columns[approxTargetIndex].Field;
          var field = type.FieldMap[interfaceField];
          var targetIndex = field.MappingInfo.Offset;
          typeColumnMap[i] = new Pair<int>(targetIndex, pair.Second);
        }

      int[] allIndexes = MaterializationHelper.CreateSingleSourceMap(descriptor.Count, typeColumnMap);
      int[] keyIndexes = Enumerable.Range(allIndexes[0], keyInfo.TupleDescriptor.Count).ToArray();

      var transform    = new MapTransform(true, descriptor, allIndexes);
      var keyTransform = new MapTransform(true, keyInfo.TupleDescriptor, keyIndexes);

      result = new TypeMapping(type, keyTransform, transform, keyIndexes);

      if (type.Hierarchy.Root.IsLeaf)
        cache.SingleItem = result;
      else
        cache.Items.Add(typeId, result);
      entityMappings[entityIndex] = cache;
      
      return result;
    }

    
    // Constructors

    public MaterializationContext(int entityCount)
    {
      model = Domain.Demand().Model;
      EntitiesInRow = entityCount;
      entityMappings = new EntityMappingCache[entityCount];
      for (int i = 0; i < entityMappings.Length; i++)
        entityMappings[i] = new EntityMappingCache {
          Items = new Dictionary<int, TypeMapping>()
        };
    }
  }
}