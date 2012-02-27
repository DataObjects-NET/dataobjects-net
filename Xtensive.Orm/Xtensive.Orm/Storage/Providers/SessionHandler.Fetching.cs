// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2009.08.19

using System.Collections.Generic;
using Xtensive.Orm;
using Xtensive.Orm.Internals.Prefetch;
using Xtensive.Orm.Model;
using TypeInfo = Xtensive.Orm.Model.TypeInfo;

namespace Xtensive.Storage.Providers
{
  partial class SessionHandler
  {
    private PrefetchManager prefetchManager;

    internal virtual int ExecutedPrefetchTasksCount { get { return prefetchManager.ExecutedTasksCount; } }

    /// <summary>
    /// Register the task prefetching fields' values of the <see cref="Entity"/> with the specified key.
    /// </summary>
    /// <param name="key">The key.</param>
    /// <param name="type">The type of the <see cref="Entity"/>.</param>
    /// <param name="descriptors">The descriptors of fields which values will be loaded.</param>
    /// <returns>A <see cref="StrongReferenceContainer"/> which can be used to save 
    /// a strong reference to a fetched <see cref="Entity"/>.</returns>
    public virtual StrongReferenceContainer Prefetch(Key key, TypeInfo type, IList<PrefetchFieldDescriptor> descriptors)
    {
      EnsureTransactionIsOpened();
      return prefetchManager.Prefetch(key, type, descriptors);
    }

    /// <summary>
    /// Executes registered prefetch tasks.
    /// </summary>
    /// <param name="skipPersist">if set to <see langword="true"/> persist is not performed.</param>
    /// <returns>A <see cref="StrongReferenceContainer"/> which can be used to save 
    /// a strong reference to a fetched <see cref="Entity"/>.</returns>
    public virtual StrongReferenceContainer ExecutePrefetchTasks(bool skipPersist)
    {
      EnsureTransactionIsOpened();
      return prefetchManager.ExecuteTasks(skipPersist);
    }

    /// <summary>
    /// Executes registered prefetch tasks.
    /// </summary>
    /// <returns>A <see cref="StrongReferenceContainer"/> which can be used to save 
    /// a strong reference to a fetched <see cref="Entity"/>.</returns>
    public StrongReferenceContainer ExecutePrefetchTasks()
    {
      return ExecutePrefetchTasks(false);
    }

    /// <summary>
    /// Fetches an <see cref="EntityState"/>.
    /// </summary>
    /// <param name="key">The key.</param>
    /// <returns>The key of fetched <see cref="EntityState"/>.</returns>
    public virtual EntityState FetchEntityState(Key key)
    {
      EnsureTransactionIsOpened();
      var type = key.TypeReference.Type;
      prefetchManager.Prefetch(key, type,
        PrefetchHelper.GetCachedDescriptorsForFieldsLoadedByDefault(Session.Domain, type));
      prefetchManager.ExecuteTasks(true);
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
      EnsureTransactionIsOpened();
      var type = key.TypeReference.Type;
      var descriptor = new PrefetchFieldDescriptor(field, false, false);
      var descriptors = new List<PrefetchFieldDescriptor> {descriptor};
      prefetchManager.Prefetch(key, type, descriptors);
      prefetchManager.ExecuteTasks(true);
    }

    /// <summary>
    /// Fetches the entity set.
    /// </summary>
    /// <param name="ownerKey">The owner key.</param>
    /// <param name="field">The field.</param>
    public virtual void FetchEntitySet(Key ownerKey, FieldInfo field, int? itemCountLimit)
    {
      EnsureTransactionIsOpened();
      var ownerType = ownerKey.TypeReference.Type;
      var descriptor = new PrefetchFieldDescriptor(field, itemCountLimit);
      var descriptors = new List<PrefetchFieldDescriptor> { descriptor };
      Session.Handler.Prefetch(ownerKey, ownerType, descriptors);
      Session.Handler.ExecutePrefetchTasks();
    }
  }
}