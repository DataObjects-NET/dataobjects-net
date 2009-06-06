// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexis Kochetov
// Created:    2009.05.29

using System.Collections.Generic;
using System.Linq;
using Xtensive.Core;
using Xtensive.Core.Tuples.Transform;
using Xtensive.Storage.Model;

namespace Xtensive.Storage.Linq.Materialization
{
  internal sealed class MaterializationContext
  {
    private readonly Dictionary<int, EntityMaterializationData>[] transformCaches;
    private readonly DomainModel model;
    public int EntitiesInRow;

    public EntityMaterializationData GetEntityMaterializationData(int entityIndex, int typeId, Pair<int>[] columns)
    {
      EntityMaterializationData result;
      var cache = transformCaches[entityIndex];
      if (cache.TryGetValue(typeId, out result))
        return result;

      var entityType = model.Types[typeId];
      var keyInfo = entityType.Hierarchy.KeyInfo;
      var descriptor = entityType.TupleDescriptor;
      int[] entityMap = MaterializationHelper.GetColumnMap(descriptor.Count, columns);
      int[] keyMap = Enumerable.Range(0, keyInfo.Length).ToArray();
      var transform = new MapTransform(true, descriptor, entityMap);
      var keyTransform = new MapTransform(true, keyInfo.TupleDescriptor, keyMap);
      result = new EntityMaterializationData(transform, keyTransform, entityType);
      cache.Add(typeId, result);
      return result;
    }

    
    // Constructors

    public MaterializationContext(int entitiesInRow)
    {
      model = Domain.Demand().Model;
      EntitiesInRow = entitiesInRow;
      transformCaches = new Dictionary<int, EntityMaterializationData>[entitiesInRow];
      for (int i = 0; i < transformCaches.Length; i++)
        transformCaches[i] = new Dictionary<int, EntityMaterializationData>();
    }
  }
}