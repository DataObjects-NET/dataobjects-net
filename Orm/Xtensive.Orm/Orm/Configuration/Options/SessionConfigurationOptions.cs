// Copyright (C) 2024 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Transactions;
using Xtensive.Core;

namespace Xtensive.Orm.Configuration.Options
{
  internal sealed class SessionConfigurationOptions :
    IIdentifyableOptions,
    IValidatableOptions,
    INamedOptionsCollectionElement
  {
    public object Identifier => Name;

    /// <summary>
    /// Session name.
    /// <see cref="SessionConfiguration.Name"/>
    /// </summary>
    public string Name { get; set; } = WellKnown.Sessions.Default;

    /// <summary>
    /// User name to authenticate.
    /// Default value is <see langword="null" />.
    /// </summary>
    public string UserName { get; set; } = string.Empty;

    /// <summary>
    /// Password to authenticate.
    /// Default value is <see langword="null" />.
    /// </summary>
    public string Password { get; set; } = string.Empty;

    /// <summary>
    /// session options.
    /// Default value is <see cref="SessionOptions.Default"/>.
    /// </summary>
    public SessionOptions Options { get; set; } = SessionOptions.Default;

    /// <summary>
    /// Size of the session entity state cache.
    /// Default value is <see cref="SessionConfiguration.DefaultCacheSize"/>.
    /// </summary>
    public int CacheSize { get; set; } = SessionConfiguration.DefaultCacheSize;

    /// <summary>
    /// Type of session cache.
    /// Default value is <see cref="SessionConfiguration.DefaultCacheType"/>.
    /// </summary>
    public SessionCacheType CacheType { get; set; } = SessionConfiguration.DefaultCacheType;

    /// <summary>
    /// Default isolation level.
    /// Default value is <see cref="SessionConfiguration.DefaultDefaultIsolationLevel"/>.
    /// </summary>
    public IsolationLevel DefaultIsolationLevel { get; set; } = SessionConfiguration.DefaultDefaultIsolationLevel;

    /// <summary>
    /// Default command timeout.
    /// Default value is <see cref="SessionConfiguration.DefaultCommandTimeout"/>.
    /// </summary>
    public int? DefaultCommandTimeout { get; set; } = null;

    /// <summary>
    /// Size of the batch.
    /// This affects create, update, delete operations and future queries.
    /// Default value is <see cref="SessionConfiguration.DefaultBatchSize"/>
    /// </summary>
    public int BatchSize { get; set; } = SessionConfiguration.DefaultBatchSize;

    /// <summary>
    /// Size of the entity change registry.
    /// Default value is <see cref="SessionConfiguration.DefaultEntityChangeRegistrySize"/>
    /// </summary>
    public int EntityChangeRegistrySize { get; set; } = SessionConfiguration.DefaultEntityChangeRegistrySize;

    /// <summary>
    /// Reader preloading policy.
    /// It affects query results reading.
    /// Default value is <see cref="SessionConfiguration.DefaultReaderPreloadingPolicy"/>.
    /// </summary>
    public ReaderPreloadingPolicy ReaderPreloading { get; set; } = SessionConfiguration.DefaultReaderPreloadingPolicy;

    /// <summary>
    /// Type of the service container
    /// </summary>
    public string ServiceContainerType { get; set; } = null;

    public string ConnectionString { get; set; } = null;
    public string ConnectionUrl { get; set; } = null;

    /// <inheritdoc/>
    /// <exception cref="ArgumentException">Name is null or empty.</exception>
    /// <exception cref="ArgumentOutOfRangeException">CacheSize, BatchSize or EntityChangeRegistry value is out of valid range.</exception>
    public void Validate()
    {
      if (Name.IsNullOrEmpty())
        throw new ArgumentException(Strings.ExNameMustBeNotNullOrEmpty);
      if (CacheSize <= 1)
        throw new ArgumentOutOfRangeException(nameof(CacheSize), CacheSize, string.Format(Strings.ExArgumentMustBeGreaterThanX, 1));
      if (BatchSize < 1)
        throw new ArgumentOutOfRangeException(nameof(BatchSize), BatchSize, string.Format(Strings.ExArgumentMustBeGreaterThatOrEqualX, 1));
      if (EntityChangeRegistrySize < 1)
        throw new ArgumentOutOfRangeException(nameof(EntityChangeRegistrySize), EntityChangeRegistrySize, string.Format(Strings.ExArgumentMustBeGreaterThatOrEqualX, 1));
    }

    /// <inheritdoc />
    public SessionConfiguration ToNative(IDictionary<string, string> connectionStrings)
    {
      // Minor hack:
      // We should not require user to specify provider name.
      // We actually know it when opening new session.
      // However, we do not know it in this method
      // We are going easy way and substituting a fake provider.
      // SQL SessionHandler is aware of this and always uses correct provider.

      var connectionInfo = ConnectionInfoParser.GetConnectionInfo(connectionStrings, ConnectionUrl, "_dummy_", ConnectionString);

      if (Name.IsNullOrEmpty()) {
        Name = WellKnown.Sessions.Default;
      }

      Validate();

      var result = new SessionConfiguration(Name) {
        UserName = UserName,
        Password = Password,
        CacheSize = CacheSize,
        BatchSize = BatchSize,
        CacheType = CacheType,
        Options = Options,
        DefaultIsolationLevel = DefaultIsolationLevel,
        ReaderPreloading = ReaderPreloading,
        ServiceContainerType = (!ServiceContainerType.IsNullOrEmpty()) ? Type.GetType(ServiceContainerType) : null,
        EntityChangeRegistrySize = EntityChangeRegistrySize,
        DefaultCommandTimeout = DefaultCommandTimeout,
        ConnectionInfo = connectionInfo,
      };
      return result;
    }
  }
}
