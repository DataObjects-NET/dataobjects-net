// Copyright (C) 2008-2020 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
// Created by: Dmitri Maximov
// Created:    2008.07.02

using System;
using System.Collections.Generic;
using Xtensive.Core;
using Xtensive.Orm.Model;
using System.Linq;

namespace Xtensive.Orm.ReferentialIntegrity
{
  internal class RemovalContext : IDisposable
  {
    private readonly RemovalProcessor processor;
    private readonly HashSet<Entity> processedEntities = new HashSet<Entity>();
    private readonly Queue<TypeInfo> types = new Queue<TypeInfo>();
    private readonly Dictionary<TypeInfo, HashSet<Entity>> queue = new Dictionary<TypeInfo, HashSet<Entity>>();
    private readonly RemovalContext parent;
    private readonly List<Action> finalizers = new List<Action>();
    private readonly Dictionary<Entity, EntityRemoveReason> removeReasons = new Dictionary<Entity, EntityRemoveReason>();

    public bool QueueIsEmpty => types.Count == 0;

    public bool Contains(Entity entity)
    {
      if (processedEntities.Contains(entity)) {
        return true;
      }

      return queue.TryGetValue(entity.TypeInfo, out var set) && set.Contains(entity)
        ? true
        : parent != null && parent.Contains(entity);
    }

    public IEnumerable<Entity> GetProcessedEntities()
      => processedEntities.Where(e => e.PersistenceState != PersistenceState.Removed);

    public List<Entity> GatherEntitiesForProcessing()
    {
      while (types.TryDequeue(out var type)) {
        var list = queue[type];
        if (list.Count == 0) {
          continue;
        }

        var result = list.Where(e => !processedEntities.Contains(e)).ToList();
        foreach (var entity in result) {
          _ = processedEntities.Add(entity);
        }

        list.Clear();
        return result;
      }
      return new List<Entity>(0);
    }

    public void Enqueue(Entity entity, EntityRemoveReason reason)
    {
      var type = entity.TypeInfo;
      if (types.Count > 0) {
        if (types.Peek() != type) {
          types.Enqueue(type);
        }
      }
      else {
        types.Enqueue(type);
      }
      if (queue.TryGetValue(type, out var set)) {
        _ = set.Add(entity);
      }
      else {
        set = new HashSet<Entity> { entity };
        queue.Add(type, set);
      }

      removeReasons[entity] = reason;
    }

    public void Enqueue(IEnumerable<Entity> entities, EntityRemoveReason reason)
    {
      foreach (var group in entities.GroupBy(e => e.TypeInfo)) {
        var type = group.Key;
        if (types.Count > 0) {
          if (types.Peek() != type) {
            types.Enqueue(type);
          }
        }
        else {
          types.Enqueue(type);
        }

        if (!queue.TryGetValue(type, out var set1)) {
          set1 = new HashSet<Entity>();
          queue.Add(type, set1);
        }
        foreach (var entity in group) {
          removeReasons[entity] = reason;
          _ = set1.Add(entity);
        }
      }
    }

    public EntityRemoveReason GetRemoveReason(Entity entity) => removeReasons[entity];

    public void EnqueueFinalizer(Action finalizer)
    {
      try {
        finalizers.Add(finalizer);
      }
// ReSharper disable EmptyGeneralCatchClause
      catch {
        // This method  should never fail
      }
// ReSharper restore EmptyGeneralCatchClause
    }

    public void ProcessFinalizers()
    {
      using (var ea = new ExceptionAggregator()) {
        // Backward order
        for (var i = finalizers.Count - 1; i >= 0; i--) {
          ea.Execute(finalizers[i]);
        }

        ea.Complete();
      }
    }

    public void Dispose()
    {
      if (parent != null) {
        foreach (var entity in processedEntities) {
          _ = parent.processedEntities.Add(entity);
        }
      }

      processedEntities.Clear();
      types.Clear();
      queue.Clear();
      removeReasons.Clear();
      processor.Context = parent;
    }

    
    // Constructors

    public RemovalContext(RemovalProcessor processor)
    {
      this.processor = processor;
      parent = processor.Context;
    }
  }
}