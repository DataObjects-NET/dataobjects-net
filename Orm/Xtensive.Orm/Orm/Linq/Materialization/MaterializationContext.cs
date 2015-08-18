// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexis Kochetov
// Created:    2009.05.29

using System;
using System.Collections.Generic;
using System.Linq;
using Xtensive.Core;
using Xtensive.Tuples.Transform;
using Xtensive.Orm.Internals;
using Xtensive.Orm.Model;

namespace Xtensive.Orm.Linq.Materialization
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

    public readonly DomainModel Model;
    public readonly Session Session;
    public readonly int EntitiesInRow;
    public readonly TypeIdRegistry TypeIdRegistry;

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

      var type       = Session.StorageNode.TypeIdRegistry[typeId];
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
      int[] keyIndexes = allIndexes.Take(keyInfo.TupleDescriptor.Count).ToArray();

      var transform    = new MapTransform(true, descriptor, allIndexes);
      var keyTransform = new MapTransform(true, keyInfo.TupleDescriptor, keyIndexes);

      result = new TypeMapping(type, keyTransform, transform, keyIndexes);

      if (type.Hierarchy.Root.IsLeaf && approximateType==type)
        cache.SingleItem = result;
      else
        cache.Items.Add(typeId, result);
      entityMappings[entityIndex] = cache;
      
      return result;
    }

    
    // Constructors

    public MaterializationContext(Session session, int entityCount)
    {
      Session = session;
      Model = session.Domain.Model;
      EntitiesInRow = entityCount;
      TypeIdRegistry = session.StorageNode.TypeIdRegistry;

      entityMappings = new EntityMappingCache[entityCount];

      for (int i = 0; i < entityMappings.Length; i++)
        entityMappings[i] = new EntityMappingCache {
          Items = new Dictionary<int, TypeMapping>()
        };
    }
  }
}