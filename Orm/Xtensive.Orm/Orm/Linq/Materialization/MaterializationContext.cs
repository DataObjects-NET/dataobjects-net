// Copyright (C) 2009-2024 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
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
      public TypeMapping? SingleItem;
      public Dictionary<int, TypeMapping> Items;
    }

    #endregion

    private readonly EntityMappingCache[] entityMappings;

    /// <summary>
    /// Gets model of current <see cref="DomainModel">domain model.</see>
    /// </summary>
    public DomainModel Model { get; private set; }

    /// <summary>
    /// Gets the session in which materialization is executing.
    /// </summary>
    public Session Session { get; private set; }

    /// <summary>
    /// Gets count of entities in query row.
    /// </summary>
    public int EntitiesInRow { get; private set; }

    /// <summary>
    /// Gets <see cref="StorageNode">node</see> specific type identifiers registry of current node.
    /// </summary>
    public TypeIdRegistry TypeIdRegistry
    {
      get {
        return Session.StorageNode.TypeIdRegistry;
      }
    }

    /// <summary>
    /// Gets or sets queue of materialization actions.
    /// </summary>
    public Queue<Action> MaterializationQueue { get; set; }

    public TypeMapping GetTypeMapping(int entityIndex, TypeInfo approximateType, int typeId, IReadOnlyList<Pair<int>> columns)
    {
      TypeMapping result;
      ref var cache = ref entityMappings[entityIndex];
      if (cache.SingleItem != null) {
        if (typeId != ResolveTypeToNodeSpecificTypeIdentifier(cache.SingleItem?.Type)) {
          throw new ArgumentOutOfRangeException("typeId");
        }
        return cache.SingleItem.Value;
      }
      if (cache.Items.TryGetValue(typeId, out result))
        return result;

      var type = TypeIdRegistry[typeId];
      var keyInfo = type.Key;
      var descriptor = type.TupleDescriptor;

      var typeColumnMap = columns;
      if (approximateType.IsInterface) {
        // fixup target index
        var newColumns = new Pair<int>[columns.Count];
        for (int i = columns.Count; i-- > 0;) {
          var pair = columns[i];
          var approxTargetIndex = pair.First;
          var interfaceField = approximateType.Columns[approxTargetIndex].Field;
          var field = type.FieldMap[interfaceField];
          var targetIndex = field.MappingInfo.Offset;
          newColumns[i] = new Pair<int>(targetIndex, pair.Second);
        }
        typeColumnMap = newColumns;
      }

      ArraySegment<int> allIndexes = MaterializationHelper.CreateSingleSourceMap(descriptor.Count, typeColumnMap);
      ArraySegment<int> keyIndexes = allIndexes.Slice(0, keyInfo.TupleDescriptor.Count);

      var transform    = new MapTransform(true, descriptor, allIndexes);
      var keyTransform = new MapTransform(true, keyInfo.TupleDescriptor, keyIndexes);

      result = new TypeMapping(type, keyTransform, transform, keyIndexes);

      if (type.Hierarchy.Root.IsLeaf && approximateType==type)
        cache.SingleItem = result;
      else
        cache.Items.Add(typeId, result);

      return result;
    }

    private int ResolveTypeToNodeSpecificTypeIdentifier(TypeInfo typeInfo)
    {
      ArgumentValidator.EnsureArgumentNotNull(typeInfo, "typeInfo");
      return TypeIdRegistry[typeInfo];
    }


    // Constructors

    public MaterializationContext(Session session, int entityCount)
    {
      Session = session;
      Model = session.Domain.Model;
      EntitiesInRow = entityCount;

      entityMappings = new EntityMappingCache[entityCount];

      for (int i = 0; i < entityMappings.Length; i++)
        entityMappings[i] = new EntityMappingCache {
          Items = new Dictionary<int, TypeMapping>()
        };
    }
  }
}