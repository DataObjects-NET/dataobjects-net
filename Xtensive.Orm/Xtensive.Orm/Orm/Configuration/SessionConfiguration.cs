// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2007.08.29

using System;
using System.Transactions;
using Xtensive.Configuration;
using Xtensive.Core;
using Xtensive.Internals.DocTemplates;

namespace Xtensive.Orm.Configuration
{
  /// <summary>
  /// <see cref="Session"/> configuration.
  /// </summary>
  /// <remarks>
  /// <para id="About"><see cref="HasStaticDefaultDocTemplate" copy="true" /></para>
  /// </remarks>
  [Serializable]
  public class SessionConfiguration : ConfigurationBase
  {
    #region Defaults

    /// <summary>
    /// Default cache size.
    /// </summary>
    public const int DefaultCacheSize = 16 * 1024;
    
    ///<summary>
    /// Default isolation level.
    ///</summary>
    public const IsolationLevel DefaultDefaultIsolationLevel = IsolationLevel.RepeatableRead;

    /// <summary>
    /// Default batch size.
    /// </summary>
    public const int DefaultBatchSize = 25;

    /// <summary>
    /// Default size of entity change registry.
    /// </summary>
    public const int DefaultEntityChangeRegistrySize = 250;

    #endregion

    /// <see cref="HasStaticDefaultDocTemplate.Default" copy="true" />
    public static readonly SessionConfiguration Default;

    private SessionOptions options = SessionOptions.Default;
    private string userName = string.Empty;
    private string password = string.Empty;
    private int cacheSize = DefaultCacheSize;
    private int batchSize = DefaultBatchSize;
    private int entityChangeRegistrySize = DefaultEntityChangeRegistrySize;
    private SessionCacheType cacheType = SessionCacheType.Default;
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
        this.EnsureNotLocked();
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
        this.EnsureNotLocked();
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
        this.EnsureNotLocked();
        ArgumentValidator.EnsureArgumentIsGreaterThan(value, 1, "CacheSize");
        cacheSize = value;
      }
    }

    /// <summary>
    /// Gets or sets the type of the session cache.
    /// </summary>
    public SessionCacheType CacheType {
      get { return cacheType; }
      set {
        this.EnsureNotLocked();
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
        this.EnsureNotLocked();
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
        this.EnsureNotLocked();
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
    /// </summary>
    public int BatchSize {
      get { return batchSize; }
      set {
        this.EnsureNotLocked();
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
        this.EnsureNotLocked();
        options = value;
      }
    }

    /// <summary>
    /// Gets or sets the reader preloading policy.
    /// </summary>
    public ReaderPreloadingPolicy ReaderPreloading
    {
      get { return readerPreloading; }
      set {
        this.EnsureNotLocked();
        readerPreloading = value;
      }
    }

    /// <summary>
    /// Gets or sets the size of the entity change registry.
    /// </summary>
    public int EntityChangeRegistrySize
    {
      get { return entityChangeRegistrySize; }
      set {
        this.EnsureNotLocked();
        entityChangeRegistrySize = value;
      }
    }

    /// <summary>
    /// Gets a value indicating whether session uses autoshortened transactions.
    /// </summary>
    public bool UseAutoShortenedTransactions {
      get { return (options & SessionOptions.AutoShortenTransactions)==SessionOptions.AutoShortenTransactions; }
      set {
        this.EnsureNotLocked();
        options = value
          ? (options | SessionOptions.AutoShortenTransactions)
          : (options & ~SessionOptions.AutoShortenTransactions);
      }
    }

    /// <summary>
    /// Gets or sets the type of the service container.
    /// </summary>
    public Type ServiceContainerType {
      get { return serviceContainerType; }
      set {
        this.EnsureNotLocked();
        serviceContainerType = value;
      }
    }

    /// <summary>
    /// Gets or sets the custom <see cref="ConnectionInfo"/> for session.
    /// </summary>
    public ConnectionInfo ConnectionInfo {
      get { return connectionInfo; }
      set {
        this.EnsureNotLocked();
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
      //   throw new InvalidOperationException(Resources.Strings.ExUnableToCloneNonUserSessionConfiguration);
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


    // Constructors

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    public SessionConfiguration()
      : this(WellKnown.Sessions.Default)
    {
    }

    /// <summary>
    /// 	<see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="sessionOptions">The session options.</param>
    public SessionConfiguration(SessionOptions sessionOptions)
      : this(null, sessionOptions)
    {
    }

    /// <summary>
    /// 	<see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="name">Value for <see cref="Name"/>.</param>
    public SessionConfiguration(string name)
      : this(name, SessionOptions.LegacyProfile)
    {
    }

    /// <summary>
    /// 	<see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
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
      Default = new SessionConfiguration(WellKnown.Sessions.Default);
      Default.Lock(true);
    }
  }
}
