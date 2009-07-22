// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexis Kochetov
// Created:    2009.05.29

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
    private readonly Dictionary<int, TypeMapping>[] transformCaches;
    private readonly DomainModel model;
    public int EntitiesInRow;

    public TypeMapping GetEntityMaterializationInfo(int entityIndex, int typeId, Pair<int>[] columns)
    {
      TypeMapping result;
      var cache = transformCaches[entityIndex];
      if (cache.TryGetValue(typeId, out result))
        return result;

      var type       = model.Types[typeId];
      var keyInfo    = type.Hierarchy.KeyInfo;
      var descriptor = type.TupleDescriptor;

      int[] allIndexes = MaterializationHelper.CreateSingleSourceMap(descriptor.Count, columns);
      int[] keyIndexes = Enumerable.Range(allIndexes[0], keyInfo.Length).ToArray();

      var transform    = new MapTransform(true, descriptor, allIndexes);
      var keyTransform = new MapTransform(true, keyInfo.TupleDescriptor, keyIndexes);

      result = new TypeMapping(type, keyTransform, transform, keyIndexes);
      cache.Add(typeId, result);
      return result;
    }

    
    // Constructors

    public MaterializationContext(int entitiesInRow)
    {
      model = Domain.Demand().Model;
      EntitiesInRow = entitiesInRow;
      transformCaches = new Dictionary<int, TypeMapping>[entitiesInRow];
      for (int i = 0; i < transformCaches.Length; i++)
        transformCaches[i] = new Dictionary<int, TypeMapping>();
    }
  }
}