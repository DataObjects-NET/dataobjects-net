// Copyright (C) 2009-2020 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
// Created by: Denis Krjuchkov
// Created:    2009.08.19

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Xtensive.Orm.Internals.Prefetch;
using Xtensive.Orm.Model;
using TypeInfo = Xtensive.Orm.Model.TypeInfo;

namespace Xtensive.Orm.Providers
{
  public partial class SqlSessionHandler
  {
    private readonly PrefetchManager prefetchManager;

    internal override int PrefetchTaskExecutionCount { get { return prefetchManager.TaskExecutionCount; } }

    /// <inheritdoc/>
    public override StrongReferenceContainer Prefetch(Key key, TypeInfo type, IReadOnlyList<PrefetchFieldDescriptor> descriptors)
    {
      return prefetchManager.Prefetch(key, type, descriptors);
    }

    /// <inheritdoc/>
    public override Task<StrongReferenceContainer> PrefetchAsync(
      Key key, TypeInfo type, IReadOnlyList<PrefetchFieldDescriptor> descriptors, CancellationToken token = default)
    {
      return prefetchManager.PrefetchAsync(key, type, descriptors, token);
    }

    /// <inheritdoc/>
    public override StrongReferenceContainer ExecutePrefetchTasks(bool skipPersist)
    {
      return prefetchManager.ExecuteTasks(skipPersist);
    }

    /// <inheritdoc/>
    public override Task<StrongReferenceContainer> ExecutePrefetchTasksAsync(bool skipPersist, CancellationToken token = default)
    {
      return prefetchManager.ExecuteTasksAsync(skipPersist, token);
    }

    /// <summary>
    /// Fetches an <see cref="EntityState"/>.
    /// </summary>
    /// <param name="key">The key.</param>
    /// <returns>The key of fetched <see cref="EntityState"/>.</returns>
    public override EntityState FetchEntityState(Key key)
    {
      var type = key.TypeReference.Type;
      prefetchManager.Prefetch(key, type,
        PrefetchHelper.GetCachedDescriptorsForFieldsLoadedByDefault(Session.Domain, type));
      prefetchManager.ExecuteTasks(true);
      return LookupState(key, out var result) ? result : null;
    }
    
    /// <summary>
    /// Fetches the field of an <see cref="Entity"/>.
    /// </summary>
    /// <param name="key">The key.</param>
    /// <param name="field">The field to fetch.</param>
    public override void FetchField(Key key, FieldInfo field)
    {
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
    /// <param name="itemCountLimit">A maximal number of items
    /// to preload while fetching <see cref="EntitySet{TItem}"/>.</param>
    public override void FetchEntitySet(Key ownerKey, FieldInfo field, int? itemCountLimit)
    {
      var ownerType = ownerKey.TypeReference.Type;
      var descriptor = new PrefetchFieldDescriptor(field, itemCountLimit);
      var descriptors = new List<PrefetchFieldDescriptor> { descriptor };
      Session.Handler.Prefetch(ownerKey, ownerType, descriptors);
      Session.Handler.ExecutePrefetchTasks();
    }
  }
}