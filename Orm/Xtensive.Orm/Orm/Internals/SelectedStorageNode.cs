using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xtensive.Core;
using Xtensive.Orm;
using Xtensive.Orm.Configuration;
using Xtensive.Orm.Interfaces;

namespace Xtensive.Orm.Internals
{
  internal sealed class SelectedStorageNode : ISelectedStorageNode
  {
    private readonly Domain domain;
    private readonly StorageNode storageNode;

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
      ArgumentValidator.EnsureArgumentNotNull(configuration, nameof(configuration));
      return domain.OpenSessionInternal(configuration, storageNode, configuration.Supports(SessionOptions.AutoActivation));
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

      return domain.OpenSessionInternalAsync(configuration,
        storageNode,
        configuration.Supports(SessionOptions.AllowSwitching),
        cancellationToken);
    }

    public SelectedStorageNode(Domain domain, StorageNode storageNode)
    {
      this.domain = domain;
      this.storageNode = storageNode;
    }
  }
}
