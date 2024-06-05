// Copyright (C) 2014-2024 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
// Created by: Denis Krjuchkov
// Created:    2014.03.13

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Xtensive.Core;
using Xtensive.Orm.Configuration;
using Xtensive.Orm.Interfaces;
using Xtensive.Orm.Model;
using Xtensive.Orm.Providers;
using Xtensive.Orm.Rse.Providers;
using Xtensive.Orm.Internals;
using Xtensive.Orm.Internals.Prefetch;

namespace Xtensive.Orm
{
  /// <summary>
  /// Storage node.
  /// </summary>
  public sealed class StorageNode : ISessionSource
  {
    private readonly Domain domain;

    /// <summary>
    /// Gets node identifier.
    /// </summary>
    public string Id { get { return Configuration.NodeId; } }

    /// <summary>
    /// Gets node configuration.
    /// </summary>
    public NodeConfiguration Configuration { get; private set; }

    /// <summary>
    /// Gets model mapping.
    /// </summary>
    public ModelMapping Mapping { get; private set; }

    /// <summary>
    /// Gets type identifier registry.
    /// </summary>
    public TypeIdRegistry TypeIdRegistry { get; private set; }

    /// <summary>
    /// Caches providers that lock certain type of entity with certain <see cref="LockMode"/> and <see cref="LockBehavior"/>.
    /// </summary>
    internal ConcurrentDictionary<(TypeInfo, LockMode, LockBehavior), ExecutableProvider> EntityLockProviderCache { get; }

    /// <summary>
    /// Caches uncompiled queries used by <see cref="PrefetchManager"/> to fetch certain entities.
    /// </summary>
    internal ConcurrentDictionary<RecordSetCacheKey, CompilableProvider> EntityFetchQueryCache { get; }

    /// <summary>
    /// Caches uncompiled queries used by <see cref="PrefetchManager"/> to fetch <see cref="EntitySet{TItem}"/> content.
    /// </summary>
    internal ConcurrentDictionary<ItemsQueryCacheKey, CompilableProvider> EntitySetFetchQueryCache { get; }

    /// <summary>
    /// Caches certain info about EntitySet fields, e.g. queries to fetch current count or items.
    /// </summary>
    internal ConcurrentDictionary<Xtensive.Orm.Model.FieldInfo, EntitySetTypeState> EntitySetTypeStateCache { get; }

    /// <summary>
    /// Caches queries that get references to entities for certain association.
    /// </summary>
    internal ConcurrentDictionary<AssociationInfo, (CompilableProvider, Parameter<Xtensive.Tuples.Tuple>)> RefsToEntityQueryCache { get; }
    internal ConcurrentDictionary<SequenceInfo, object> KeySequencesCache { get; }
    internal ConcurrentDictionary<PersistRequestBuilderTask, IReadOnlyCollection<PersistRequest>> PersistRequestCache { get; private set; }

    /// <inheritdoc/>
    public Session OpenSession() =>
      OpenSession(domain.Configuration.Sessions.Default);

    /// <inheritdoc/>
    public Session OpenSession(SessionType type) =>
      OpenSession(domain.GetSessionConfiguration(type));

    /// <inheritdoc/>
    public Session OpenSession(SessionConfiguration configuration) =>
      domain.OpenSessionInternal(configuration, this, configuration.Supports(SessionOptions.AutoActivation));

    /// <inheritdoc/>
    public Task<Session> OpenSessionAsync(CancellationToken cancellationToken = default) =>
      OpenSessionAsync(domain.Configuration.Sessions.Default, cancellationToken);

    /// <inheritdoc/>
    public Task<Session> OpenSessionAsync(SessionType type, CancellationToken cancellationToken = default) =>
      OpenSessionAsync(domain.GetSessionConfiguration(type), cancellationToken);

    /// <inheritdoc/>
    public Task<Session> OpenSessionAsync(SessionConfiguration configuration, CancellationToken cancellationToken = default)
    {
      ArgumentValidator.EnsureArgumentNotNull(configuration, nameof(configuration));

      SessionScope sessionScope = null;
      try {
        if (configuration.Supports(SessionOptions.AutoActivation)) {
          sessionScope = new SessionScope();
        }
        return domain.OpenSessionInternalAsync(configuration, this, sessionScope, cancellationToken);
      }
      catch {
        sessionScope?.Dispose();
        throw;
      }
    }

    // Constructors

    internal StorageNode(Domain domain, NodeConfiguration configuration, ModelMapping mapping, TypeIdRegistry typeIdRegistry)
    {
      ArgumentValidator.EnsureArgumentNotNull(domain, nameof(domain));
      ArgumentValidator.EnsureArgumentNotNull(configuration, nameof(configuration));
      ArgumentValidator.EnsureArgumentNotNull(mapping, nameof(mapping));
      ArgumentValidator.EnsureArgumentNotNull(typeIdRegistry, nameof(typeIdRegistry));

      EntityLockProviderCache = new ConcurrentDictionary<(TypeInfo, LockMode, LockBehavior), ExecutableProvider>();
      EntityFetchQueryCache = new ConcurrentDictionary<RecordSetCacheKey, CompilableProvider>();
      EntitySetFetchQueryCache = new ConcurrentDictionary<ItemsQueryCacheKey, CompilableProvider>();
      EntitySetTypeStateCache = new ConcurrentDictionary<Xtensive.Orm.Model.FieldInfo, EntitySetTypeState>();
      RefsToEntityQueryCache = new ConcurrentDictionary<AssociationInfo, (CompilableProvider, Parameter<Xtensive.Tuples.Tuple>)>();
      KeySequencesCache = new ConcurrentDictionary<SequenceInfo, object>();
      PersistRequestCache = new ConcurrentDictionary<PersistRequestBuilderTask, IReadOnlyCollection<PersistRequest>>();

      this.domain = domain;
      Configuration = configuration;
      Mapping = mapping;
      TypeIdRegistry = typeIdRegistry;
    }
  }
}
