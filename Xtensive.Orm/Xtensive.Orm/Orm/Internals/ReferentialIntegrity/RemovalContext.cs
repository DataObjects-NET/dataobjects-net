// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
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

    public bool QueueIsEmpty
    {
      get { return types.Count == 0; }
    }

    public bool Contains(Entity entity)
    {
      if (processedEntities.Contains(entity))
        return true;
      HashSet<Entity> set;
      if (queue.TryGetValue(entity.TypeInfo, out set) && set.Contains(entity))
        return true;
      return parent != null && parent.Contains(entity);
    }

    public IEnumerable<Entity> GetProcessedEntities()
    {
      return processedEntities.Where(e => e.PersistenceState != PersistenceState.Removed);
    }

    public List<Entity> GatherEntitiesForProcessing()
    {
      while (types.Count > 0) {
        var type = types.Dequeue();
        var list = queue[type];
        if (list.Count == 0) 
          continue;
        var result = list.Where(e => !processedEntities.Contains(e)).ToList();
        foreach (var entity in result)
          processedEntities.Add(entity);
        list.Clear();
        return result;
      }
      return new List<Entity>(0);
    }

    public void Enqueue(Entity entity)
    {
      var type = entity.TypeInfo;
      if (types.Count > 0) {
        if (types.Peek() != type)
          types.Enqueue(type);
      }
      else
        types.Enqueue(type);
      HashSet<Entity> set;
      if (queue.TryGetValue(type, out set))
        set.Add(entity);
      else {
        set = new HashSet<Entity> {entity};
        queue.Add(type, set);
      }
    }

    public void Enqueue(IEnumerable<Entity> entities)
    {
      foreach (var group in entities.GroupBy(e => e.TypeInfo)) {
        var type = group.Key;
        if (types.Count > 0) {
          if (types.Peek() != type)
            types.Enqueue(type);
        }
        else
          types.Enqueue(type);
        HashSet<Entity> set;
        if (queue.TryGetValue(type, out set))
          foreach (var entity in group)
            set.Add(entity);
        else {
          set = new HashSet<Entity>(group);
          queue.Add(type, set);
        }
      }
    }

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
        for (int i = finalizers.Count - 1; i >= 0; i--)
          ea.Execute(finalizers[i]);
        ea.Complete();
      }
    }

    public void Dispose()
    {
      if (parent != null)
        foreach (var entity in processedEntities)
          parent.processedEntities.Add(entity);
      processedEntities.Clear();
      types.Clear();
      queue.Clear();
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