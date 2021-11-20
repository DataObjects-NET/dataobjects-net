// Copyright (C) 2014-2021 Xtensive LLC.
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

    internal ConcurrentDictionary<(TypeInfo, LockMode, LockBehavior), ExecutableProvider> InternalExecutableProviderCache { get; } =
      new ConcurrentDictionary<(TypeInfo, LockMode, LockBehavior), ExecutableProvider>();

    internal ConcurrentDictionary<RecordSetCacheKey, CompilableProvider> InternalRecordSetCache { get; } =
      new ConcurrentDictionary<RecordSetCacheKey, CompilableProvider>();

    internal ConcurrentDictionary<ItemsQueryCacheKey, CompilableProvider> InternalItemsQueryCache { get; } =
      new ConcurrentDictionary<ItemsQueryCacheKey, CompilableProvider>();

    internal ConcurrentDictionary<Xtensive.Orm.Model.FieldInfo, EntitySetTypeState> InternalEntitySetCache { get; } =
      new ConcurrentDictionary<Xtensive.Orm.Model.FieldInfo, EntitySetTypeState>();

    internal ConcurrentDictionary<AssociationInfo, (CompilableProvider, Parameter<Xtensive.Tuples.Tuple>)> InternalAssociationCache { get; } =
      new ConcurrentDictionary<AssociationInfo, (CompilableProvider, Parameter<Xtensive.Tuples.Tuple>)>();

    internal ConcurrentDictionary<SequenceInfo, object> KeySequencesCache { get; } = new ConcurrentDictionary<SequenceInfo, object>();

    internal ConcurrentDictionary<PersistRequestBuilderTask, ICollection<PersistRequest>> PersistRequestCache { get; }
      = new ConcurrentDictionary<PersistRequestBuilderTask, ICollection<PersistRequest>>();

    /// <inheritdoc/>
    public Session OpenSession()
    {
      return OpenSession(domain.Configuration.Sessions.Default);
    }

    /// <inheritdoc/>
    public Session OpenSession(SessionType type)
    {
      return type switch {
        SessionType.User => OpenSession(domain.Configuration.Sessions.Default),
        SessionType.System => OpenSession(domain.Configuration.Sessions.System),
        SessionType.KeyGenerator => OpenSession(domain.Configuration.Sessions.KeyGenerator),
        SessionType.Service => OpenSession(domain.Configuration.Sessions.Service),
        _ => throw new ArgumentOutOfRangeException("type"),
      };
    }

    /// <inheritdoc/>
    public Session OpenSession(SessionConfiguration configuration)
    {
      return domain.OpenSessionInternal(configuration, this, configuration.Supports(SessionOptions.AutoActivation));
    }

    /// <inheritdoc/>
    public Task<Session> OpenSessionAsync(CancellationToken cancellationToken = default) =>
      OpenSessionAsync(domain.Configuration.Sessions.Default, cancellationToken);

    /// <inheritdoc/>
    public Task<Session> OpenSessionAsync(SessionType type, CancellationToken cancellationToken = default)
    {
      return type switch {
        SessionType.User => OpenSessionAsync(domain.Configuration.Sessions.Default),
        SessionType.System => OpenSessionAsync(domain.Configuration.Sessions.System),
        SessionType.KeyGenerator => OpenSessionAsync(domain.Configuration.Sessions.KeyGenerator),
        SessionType.Service => OpenSessionAsync(domain.Configuration.Sessions.Service),
        _ => throw new ArgumentOutOfRangeException("type"),
      };
    }

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

      this.domain = domain;
      Configuration = configuration;
      Mapping = mapping;
      TypeIdRegistry = typeIdRegistry;
    }
  }
}
