// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexander Nikolaev
// Created:    2009.10.20

using System;
using System.Collections.Generic;
using Xtensive.Core;
using Xtensive.Core.Collections;

namespace Xtensive.Storage.Internals.Prefetch
{
  [Serializable]
  internal sealed class Fetcher
  {
    private readonly SetSlim<EntityGroupTask> tasks = new SetSlim<EntityGroupTask>();
    private readonly HashSet<Key> foundedKeys = new HashSet<Key>();

    private readonly PrefetchProcessor processor;

    public void ExecuteTasks(SetSlim<GraphContainer> containers)
    {
      try {
        foreach (var container in containers)
          if (container.RootEntityContainer!=null)
            AddTask(container.RootEntityContainer);
        RegisterAllEntityGroupTasks();
        RegisterAllEntitySetTasks(containers);

        processor.Owner.Session.ExecuteAllDelayedQueries(false);
        UpdateCacheFromAllEntityGroupTasks();
        UpdateCacheFromAllEntitySetTasks(containers);

        tasks.Clear();
        
        foreach (var container in containers) {
          var referencedEntityPrefetchContainers = container.ReferencedEntityContainers;
          if (referencedEntityPrefetchContainers!=null)
            foreach (var referencedEntityPrefetchContainer in referencedEntityPrefetchContainers)
              AddTask(referencedEntityPrefetchContainer);
        }
        RegisterAllEntityGroupTasks();

        processor.Owner.Session.ExecuteAllDelayedQueries(false);
        UpdateCacheFromAllEntityGroupTasks();
      }
      finally {
        tasks.Clear();
        foundedKeys.Clear();
      }
    }

    private static void UpdateCacheFromAllEntitySetTasks(IEnumerable<GraphContainer> containers)
    {
      foreach (var container in containers) {
        var entitySetPrefetchTasks = container.EntitySetTasks;
        if (entitySetPrefetchTasks!=null) {
          foreach (var entitySetPrefetchTask in entitySetPrefetchTasks)
            entitySetPrefetchTask.UpdateCache();
        }
      }
    }

    private static void RegisterAllEntitySetTasks(IEnumerable<GraphContainer> containers)
    {
      foreach (var container in containers) {
        var entitySetPrefetchTasks = container.EntitySetTasks;
        if (entitySetPrefetchTasks!=null) {
          foreach (var entitySetPrefetchTask in entitySetPrefetchTasks)
            entitySetPrefetchTask.RegisterQueryTask();
        }
      }
    }

    private void AddTask(EntityContainer container)
    {
      var newTask = container.GetTask();
      if (newTask!=null) {
        var existingTask = tasks[newTask];
        if (existingTask == null) {
          tasks.Add(newTask);
          existingTask = newTask;
        }
        existingTask.AddKey(container.Key, container.ExactType);
      }
    }

    private void UpdateCacheFromAllEntityGroupTasks()
    {
      foreach (var task in tasks)
        task.UpdateCache(foundedKeys);
    }

    private void RegisterAllEntityGroupTasks()
    {
      foreach (var task in tasks)
        task.RegisterQueryTasks();
    }


    // Constructors

    public Fetcher(PrefetchProcessor processor)
    {
      ArgumentValidator.EnsureArgumentNotNull(processor, "processor");

      this.processor = processor;
    }
  }
}