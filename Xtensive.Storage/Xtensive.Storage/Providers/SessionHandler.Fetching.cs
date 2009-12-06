// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2009.08.19

using Xtensive.Storage.Internals.Prefetch;
using Xtensive.Storage.Model;

namespace Xtensive.Storage.Providers
{
  partial class SessionHandler
  {
    private PrefetchManager prefetchManager;

    internal virtual int PrefetchTaskExecutionCount { get { return prefetchManager.TaskExecutionCount; } }

    /// <summary>
    /// Register the task prefetching fields' values of the <see cref="Entity"/> with the specified key.
    /// </summary>
    /// <param name="key">The key.</param>
    /// <param name="type">The type of the <see cref="Entity"/>.</param>
    /// <param name="descriptors">The descriptors of fields which values will be loaded.</param>
    /// <returns>A <see cref="StrongReferenceContainer"/> which can be used to save 
    /// a strong reference to a fetched <see cref="Entity"/>.</returns>
    public virtual StrongReferenceContainer Prefetch(Key key, TypeInfo type,
      FieldDescriptorCollection descriptors)
    {
      return prefetchManager.Prefetch(key, type, descriptors);
    }

    /// <summary>
    /// Executes registered prefetch tasks.
    /// </summary>
    /// <returns>A <see cref="StrongReferenceContainer"/> which can be used to save 
    /// a strong reference to a fetched <see cref="Entity"/>.</returns>
    public virtual StrongReferenceContainer ExecutePrefetchTasks()
    {
      return prefetchManager.ExecuteTasks();
    }

    /// <summary>
    /// Fetches an <see cref="EntityState"/>.
    /// </summary>
    /// <param name="key">The key.</param>
    /// <returns>The key of fetched <see cref="EntityState"/>.</returns>
    public virtual EntityState FetchInstance(Key key)
    {
      var type = key.TypeRef.Type;
      prefetchManager.Prefetch(key, type,
        PrefetchHelper.GetCachedDescriptorsForFieldsLoadedByDefault(Session.Domain, type));
      prefetchManager.ExecuteTasks();
      EntityState result;
      return TryGetEntityState(key, out result) ? result : null;
    }

    /// <summary>
    /// Fetches the field of an <see cref="Entity"/>.
    /// </summary>
    /// <param name="key">The key.</param>
    /// <param name="field">The field to fetch.</param>
    public virtual void FetchField(Key key, FieldInfo field)
    {
      var type = key.TypeRef.Type;
      prefetchManager.Prefetch(key, type,
        new FieldDescriptorCollection(new PrefetchFieldDescriptor(field, false, false)));
      prefetchManager.ExecuteTasks();
    }
  }
}