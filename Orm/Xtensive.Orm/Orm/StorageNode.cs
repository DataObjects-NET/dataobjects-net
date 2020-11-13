// Copyright (C) 2014-2020 Xtensive LLC.
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

    internal ConcurrentDictionary<object, object> InternalQueryCache { get; private set; }

    internal ConcurrentDictionary<SequenceInfo, object> KeySequencesCache { get; private set; }

    internal ConcurrentDictionary<PersistRequestBuilderTask, ICollection<PersistRequest>> PersistRequestCache { get; private set; }

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
        return domain.OpenSessionInternalAsync(configuration, null, sessionScope, cancellationToken);
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

      KeySequencesCache = new ConcurrentDictionary<SequenceInfo, object>();
      PersistRequestCache = new ConcurrentDictionary<PersistRequestBuilderTask, ICollection<PersistRequest>>();
      InternalQueryCache = new ConcurrentDictionary<object, object>();
    }
  }
}