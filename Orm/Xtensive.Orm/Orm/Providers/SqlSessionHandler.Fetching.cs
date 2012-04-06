// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2009.08.19

using System.Collections.Generic;
using Xtensive.Orm.Internals.Prefetch;
using Xtensive.Orm.Model;
using TypeInfo = Xtensive.Orm.Model.TypeInfo;

namespace Xtensive.Orm.Providers
{
  partial class SqlSessionHandler
  {
    private readonly PrefetchManager prefetchManager;

    internal override int PrefetchTaskExecutionCount { get { return prefetchManager.TaskExecutionCount; } }

    public override StrongReferenceContainer Prefetch(Key key, TypeInfo type, IList<PrefetchFieldDescriptor> descriptors)
    {
      EnsureTransactionIsOpened();
      return prefetchManager.Prefetch(key, type, descriptors);
    }

    public override StrongReferenceContainer ExecutePrefetchTasks(bool skipPersist)
    {
      EnsureTransactionIsOpened();
      return prefetchManager.ExecuteTasks(skipPersist);
    }

    /// <summary>
    /// Fetches an <see cref="EntityState"/>.
    /// </summary>
    /// <param name="key">The key.</param>
    /// <returns>The key of fetched <see cref="EntityState"/>.</returns>
    public override EntityState FetchEntityState(Key key)
    {
      EnsureTransactionIsOpened();
      var type = key.TypeReference.Type;
      prefetchManager.Prefetch(key, type,
        PrefetchHelper.GetCachedDescriptorsForFieldsLoadedByDefault(Session.Domain, type));
      prefetchManager.ExecuteTasks(true);
      EntityState result;
      return LookupState(key, out result) ? result : null;
    }
    
    /// <summary>
    /// Fetches the field of an <see cref="Entity"/>.
    /// </summary>
    /// <param name="key">The key.</param>
    /// <param name="field">The field to fetch.</param>
    public override void FetchField(Key key, FieldInfo field)
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
    public override void FetchEntitySet(Key ownerKey, FieldInfo field, int? itemCountLimit)
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