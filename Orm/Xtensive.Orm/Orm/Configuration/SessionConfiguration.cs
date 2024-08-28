// Copyright (C) 2007-2020 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
// Created by: Dmitri Maximov
// Created:    2007.08.29

using System;
using System.Transactions;
using Xtensive.Core;


namespace Xtensive.Orm.Configuration
{
  /// <summary>
  /// <see cref="Session"/> configuration.
  /// </summary>
  [Serializable]
  public class SessionConfiguration : ConfigurationBase
  {
    #region Defaults

    /// <summary>
    /// Default cache size.
    /// </summary>
    public const int DefaultCacheSize = 16 * 1024;

    /// <summary>
    /// Default session options.
    /// </summary>
    public const SessionOptions DefaultSessionOptions = SessionOptions.Default;

    ///<summary>
    /// Default isolation level.
    ///</summary>
    public const IsolationLevel DefaultDefaultIsolationLevel = IsolationLevel.RepeatableRead;

    /// <summary>
    /// Default cache type.
    /// </summary>
    public const SessionCacheType DefaultCacheType = SessionCacheType.Default;

    /// <summary>
    /// Default reader preloading policy.
    /// </summary>
    public const ReaderPreloadingPolicy DefaultReaderPreloadingPolicy = ReaderPreloadingPolicy.Default;

    /// <summary>
    /// Default batch size.
    /// </summary>
    public const int DefaultBatchSize = 25;

    /// <summary>
    /// Default size of entity change registry.
    /// </summary>
    public const int DefaultEntityChangeRegistrySize = 250;

    #endregion

    /// <summary>
    /// Default <see cref="SessionConfiguration"/>.
    /// </summary>
    public static readonly SessionConfiguration Default = new SessionConfiguration(WellKnown.Sessions.Default);

    private SessionOptions options = DefaultSessionOptions;
    private string userName = string.Empty;
    private string password = string.Empty;
    private int cacheSize = DefaultCacheSize;
    private int batchSize = DefaultBatchSize;
    private int entityChangeRegistrySize = DefaultEntityChangeRegistrySize;
    private SessionCacheType cacheType = DefaultCacheType;
    private IsolationLevel defaultIsolationLevel = DefaultDefaultIsolationLevel; // what a fancy name?
    private int? defaultCommandTimeout = null;
    private ReaderPreloadingPolicy readerPreloading = ReaderPreloadingPolicy.Default;
    private Type serviceContainerType;
    private ConnectionInfo connectionInfo;

    /// <summary>
    /// Gets the session name.
    /// </summary>
    public string Name { get; private set; }

    /// <summary>
    /// Gets or sets user name to authenticate.
    /// Default value is <see langword="null" />.
    /// </summary>
    public string UserName {
      get { return userName; }
      set {
        EnsureNotLocked();
        userName = value;
      }
    }

    /// <summary>
    /// Gets or sets password to authenticate.
    /// Default value is <see langword="null" />.
    /// </summary>
    public string Password {
      get { return password; }
      set {
        EnsureNotLocked();
        password = value;
      }
    }

    /// <summary>
    /// Gets or sets the size of the session entity state cache. 
    /// Default value is <see cref="DefaultCacheSize"/>.
    /// </summary>
    public int CacheSize {
      get { return cacheSize; }
      set {
        EnsureNotLocked();
        ArgumentValidator.EnsureArgumentIsGreaterThan(value, 1, "CacheSize");
        cacheSize = value;
      }
    }

    /// <summary>
    /// Gets or sets the type of the session cache.
    /// Default value is <see cref="DefaultCacheType"/>.
    /// </summary>
    public SessionCacheType CacheType {
      get { return cacheType; }
      set {
        EnsureNotLocked();
        cacheType = value;
      }
    }

    /// <summary>
    /// Gets or sets the default isolation level. 
    /// Default value is <see cref="DefaultDefaultIsolationLevel"/>.
    /// </summary>
    public IsolationLevel DefaultIsolationLevel {
      get { return defaultIsolationLevel; }
      set {
        EnsureNotLocked();
        defaultIsolationLevel = value;
      }
    }

    /// <summary>
    /// Gets or sets the default command timeout.
    /// Default value is <see cref="DefaultCommandTimeout"/>.
    /// </summary>
    public int? DefaultCommandTimeout {
      get { return defaultCommandTimeout; }
      set {
        EnsureNotLocked();
        defaultCommandTimeout = value;
      }
    }

    /// <summary>
    /// Gets session type.
    /// Default value is <see cref="SessionType.User"/>.
    /// </summary>
    public SessionType Type { get; private set; }

    /// <summary>
    /// Gets or sets the size of the batch.
    /// This affects create, update, delete operations and future queries.
    /// Default value is <see cref="DefaultBatchSize"/>.
    /// </summary>
    public int BatchSize {
      get { return batchSize; }
      set {
        EnsureNotLocked();
        batchSize = value;
      }
    }

    /// <summary>
    /// Gets or sets session options.
    /// Default value is <see cref="SessionOptions.Default"/>.
    /// </summary>
    public SessionOptions Options {
      get { return options; }
      set {
        EnsureNotLocked();
        options = value;
      }
    }

    /// <summary>
    /// Gets or sets the reader preloading policy.
    /// It affects query results reading.
    /// Default value is <see cref="DefaultReaderPreloadingPolicy"/>.
    /// </summary>
    public ReaderPreloadingPolicy ReaderPreloading
    {
      get { return readerPreloading; }
      set {
        EnsureNotLocked();
        readerPreloading = value;
      }
    }

    /// <summary>
    /// Gets or sets the size of the entity change registry.
    /// Default value is <see cref="DefaultEntityChangeRegistrySize"/>.
    /// </summary>
    public int EntityChangeRegistrySize
    {
      get { return entityChangeRegistrySize; }
      set {
        EnsureNotLocked();
        entityChangeRegistrySize = value;
      }
    }

    /// <summary>
    /// Gets or sets the type of the service container.
    /// </summary>
    public Type ServiceContainerType {
      get { return serviceContainerType; }
      set {
        EnsureNotLocked();
        serviceContainerType = value;
      }
    }

    /// <summary>
    /// Gets or sets the custom <see cref="ConnectionInfo"/> for session.
    /// </summary>
    public ConnectionInfo ConnectionInfo {
      get { return connectionInfo; }
      set {
        EnsureNotLocked();
        connectionInfo = value;
      }
    }

    /// <inheritdoc/>
    public override void Validate()
    {
      ArgumentValidator.EnsureArgumentIsGreaterThanOrEqual(CacheSize, 0, "CacheSize");
      if (DefaultCommandTimeout!=null)
        ArgumentValidator.EnsureArgumentIsGreaterThanOrEqual(DefaultCommandTimeout.Value, 0, "DefaultCommandTimeout");
    }

    /// <inheritdoc/>
    protected override ConfigurationBase CreateClone()
    {
      // Currently disabled
      // if (Type != SessionType.User)
      //   throw new InvalidOperationException(Strings.ExUnableToCloneNonUserSessionConfiguration);
      return new SessionConfiguration(Name, Options);
    }

    /// <inheritdoc/>
    protected override void CopyFrom(ConfigurationBase source)
    {
      base.CopyFrom(source);
      var configuration = (SessionConfiguration) source;
      UserName = configuration.UserName;
      Password = configuration.Password;
      options = configuration.options;
      CacheType = configuration.CacheType;
      CacheSize = configuration.CacheSize;
      BatchSize = configuration.BatchSize;
      EntityChangeRegistrySize = configuration.EntityChangeRegistrySize;
      ReaderPreloading = configuration.readerPreloading;
      DefaultIsolationLevel = configuration.DefaultIsolationLevel;
      DefaultCommandTimeout = configuration.DefaultCommandTimeout;
      ServiceContainerType = configuration.ServiceContainerType;
      ConnectionInfo = configuration.connectionInfo;
    }

    /// <summary>
    /// Clones this configuration.
    /// </summary>
    /// <returns>The clone of this configuration.</returns>
    public new SessionConfiguration Clone()
    {
      return (SessionConfiguration) base.Clone();
    }

    /// <inheritdoc/>
    public override string ToString()
    {
      var safeUserName = string.IsNullOrEmpty(UserName) ? "<null>" : UserName;
      return string.Format(
        "Name = {0}, UserName = {1}, Options = {2}, CacheType = {3}, CacheSize = {4}, DefaultIsolationLevel = {5}",
        Name, safeUserName, Options, CacheType, CacheSize, DefaultIsolationLevel);
    }

    internal bool Supports(SessionOptions required)
    {
      return (options & required)==required;
    }


    // Constructors

    /// <summary>
    /// Initializes a new instance of this class.
    /// </summary>
    public SessionConfiguration()
      : this(WellKnown.Sessions.Default)
    {
    }

    /// <summary>
    /// 	Initializes a new instance of this class.
    /// </summary>
    /// <param name="sessionOptions">The session options.</param>
    public SessionConfiguration(SessionOptions sessionOptions)
      : this(WellKnown.Sessions.Default, sessionOptions)
    {}

    /// <summary>
    /// 	Initializes a new instance of this class.
    /// </summary>
    /// <param name="name">Value for <see cref="Name"/>.</param>
    public SessionConfiguration(string name)
      : this(name, SessionOptions.ServerProfile)
    {}

    /// <summary>
    /// 	Initializes a new instance of this class.
    /// </summary>
    /// <param name="sessionOptions">The session options.</param>
    /// <param name="name">Value for <see cref="Name"/>.</param>
    public SessionConfiguration(string name, SessionOptions sessionOptions)
    {
      ArgumentValidator.EnsureArgumentNotNullOrEmpty(name, "name");

      Name = name;
      Options = sessionOptions;
      switch (name) {
      case WellKnown.Sessions.System:
        Type = SessionType.System;
        break;
      case WellKnown.Sessions.Service:
        Type = SessionType.Service;
        break;
      case WellKnown.Sessions.KeyGenerator:
        Type = SessionType.KeyGenerator;
        break;
      default:
        Type = SessionType.User;
        break;
      }
    }


    // Type initializer

    static SessionConfiguration()
    {
      Default.Lock(true);
    }
  }
}
