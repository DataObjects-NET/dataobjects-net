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
  partial class SessionHandler
  {
    internal abstract int PrefetchTaskExecutionCount { get; }

    /// <summary>
    /// Register the task prefetching fields' values of the <see cref="Entity"/> with the specified key.
    /// </summary>
    /// <param name="key">The key.</param>
    /// <param name="type">The type of the <see cref="Entity"/>.</param>
    /// <param name="descriptors">The descriptors of fields which values will be loaded.</param>
    /// <returns>A <see cref="StrongReferenceContainer"/> which can be used to save 
    /// a strong reference to a fetched <see cref="Entity"/>.</returns>
    public abstract StrongReferenceContainer Prefetch(Key key, TypeInfo type, IReadOnlyList<PrefetchFieldDescriptor> descriptors);

    /// <summary>
    /// Registers a task prefetching field values of the <see cref="Entity"/> with the specified key.
    /// </summary>
    /// <remarks>Multiple active operations in the same session instance are not supported. Use
    /// <see langword="await"/> to ensure that all asynchronous operations have completed before calling
    /// another method in this session.</remarks>
    /// <param name="key">The key.</param>
    /// <param name="type">The type of the <see cref="Entity"/>.</param>
    /// <param name="descriptors">The descriptors of fields which values will be loaded.</param>
    /// <param name="token">The token to cancel this operation</param>
    /// <returns>A <see cref="StrongReferenceContainer"/> which can be used to save
    /// a strong reference to a fetched <see cref="Entity"/>.</returns>
    public abstract Task<StrongReferenceContainer> PrefetchAsync(
      Key key, TypeInfo type, IReadOnlyList<PrefetchFieldDescriptor> descriptors, CancellationToken token = default);

    /// <summary>
    /// Executes registered prefetch tasks.
    /// </summary>
    /// <param name="skipPersist">if set to <see langword="true"/> persist is not performed.</param>
    /// <returns>A <see cref="StrongReferenceContainer"/> which can be used to save 
    /// a strong reference to a fetched <see cref="Entity"/>.</returns>
    public abstract StrongReferenceContainer ExecutePrefetchTasks(bool skipPersist);

    /// <summary>
    /// Executes registered prefetch tasks.
    /// </summary>
    /// <remarks>Multiple active operations in the same session instance are not supported. Use
    /// <see langword="await"/> to ensure that all asynchronous operations have completed before calling
    /// another method in this session.</remarks>
    /// <param name="skipPersist">if set to <see langword="true"/> persist is not performed.</param>
    /// <param name="token">The token to cancel this operation</param>
    /// <returns>A <see cref="StrongReferenceContainer"/> which can be used to save
    /// a strong reference to a fetched <see cref="Entity"/>.</returns>
    public abstract Task<StrongReferenceContainer> ExecutePrefetchTasksAsync(
      bool skipPersist, CancellationToken token = default);

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
    /// Executes registered prefetch tasks.
    /// </summary>
    /// <remarks>Multiple active operations in the same session instance are not supported. Use
    /// <see langword="await"/> to ensure that all asynchronous operations have completed before calling
    /// another method in this session.</remarks>
    /// <returns>A <see cref="StrongReferenceContainer"/> which can be used to save
    /// a strong reference to a fetched <see cref="Entity"/>.</returns>
    public Task<StrongReferenceContainer> ExecutePrefetchTasksAsync(CancellationToken token = default)
    {
      return ExecutePrefetchTasksAsync(false, token);
    }

    /// <summary>
    /// Fetches an <see cref="EntityState"/>.
    /// </summary>
    /// <param name="key">The key.</param>
    /// <returns>The key of fetched <see cref="EntityState"/>.</returns>
    public abstract EntityState FetchEntityState(Key key);

    /// <summary>
    /// Fetches the field of an <see cref="Entity"/>.
    /// </summary>
    /// <param name="key">The key.</param>
    /// <param name="field">The field to fetch.</param>
    public abstract void FetchField(Key key, FieldInfo field);

    /// <summary>
    /// Fetches the entity set.
    /// </summary>
    /// <param name="ownerKey">The owner key.</param>
    /// <param name="field">The field.</param>
    /// <param name="itemCountLimit">A maximal number of items
    /// to preload while fetching <see cref="EntitySet{TItem}"/>.</param>
    public abstract void FetchEntitySet(Key ownerKey, FieldInfo field, int? itemCountLimit);
  }
}