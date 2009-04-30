// Copyright (C) 2007 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2007.08.29

using System;
using System.Transactions;
using Xtensive.Core;
using Xtensive.Core.Helpers;
using Xtensive.Core.Internals.DocTemplates;

namespace Xtensive.Storage.Configuration
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
    #region Defaults (constants)

    /// <summary>
    /// Default cache size.
    /// </summary>
    public const int DefaultCacheSize = 16 * 1024;

    /// <summary>
    /// Default cache type.
    /// </summary>
    public const SessionCacheType DefaultCacheType = SessionCacheType.LruWeak;

    ///<summary>
    /// Default isolation level.
    ///</summary>
    public const IsolationLevel DefaultIsolationLevelValue = IsolationLevel.ReadCommitted;

    #endregion

    /// <see cref="HasStaticDefaultDocTemplate.Default" copy="true" />
    public static readonly SessionConfiguration Default;
    
    private SessionOptions options;
    private SessionType type = SessionType.User;
    private string name;
    private string userName;
    private string password;
    private int cacheSize;
    private SessionCacheType cacheType;
    private IsolationLevel defaultIsolationLevel;


    /// <summary>
    /// Gets or sets the session name.
    /// Default value is <see langword="null" />.
    /// </summary>
    public string Name
    {
      get { return name; }
      set
      {
        this.EnsureNotLocked();
        ArgumentValidator.EnsureArgumentNotNull(value, "Name");
        name = value;
      }
    }

    /// <summary>
    /// Gets or sets user name to authenticate.
    /// Default value is <see langword="null" />.
    /// </summary>
    public string UserName
    {
      get { return userName; }
      set
      {
              this.EnsureNotLocked();
        userName = value;
      }
    }

    /// <summary>
    /// Gets or sets password to authenticate.
    /// Default value is <see langword="null" />.
    /// </summary>
    public string Password
    {
      get { return password; }
      set
      {
        this.EnsureNotLocked();
        password = value;
      }
    }

    /// <summary>
    /// Gets or sets the size of the session cache. 
    /// Default value is <see cref="DefaultCacheSize"/>.
    /// </summary>
    public int CacheSize
    {
      get { return cacheSize; }
      set
      {
        this.EnsureNotLocked();
        ArgumentValidator.EnsureArgumentIsInRange(value, 0, Int32.MaxValue, "CacheSize");
        cacheSize = value;
      }
    }

    /// <summary>
    /// Gets or sets the type of the session cache.
    /// Default value is <see cref="DefaultCacheType"/>.
    /// </summary>
    /// <value>The type of the cache.</value>
    public SessionCacheType CacheType
    {
      get { return cacheType; }
      set
      {
        this.EnsureNotLocked();
        cacheType = value;
      }
    }

    /// <summary>
    /// Gets or sets the default isolation level. 
    /// Default value is <see cref="DefaultIsolationLevelValue"/>.
    /// </summary>
    public IsolationLevel DefaultIsolationLevel
    {
      get { return defaultIsolationLevel; }
      set 
      {
        this.EnsureNotLocked();
        defaultIsolationLevel = value;
      }
    }

    /// <summary>
    /// Gets session type.
    /// Default value is <see cref="SessionType.Default"/>.
    /// </summary>
    public SessionType Type
    {
      get { return type; }
      internal set { type = value; }
    }

    /// <summary>
    /// Gets or sets session options.
    /// Default value is <see cref="SessionOptions.Default"/>.
    /// </summary>
    public SessionOptions Options
    {
      get { return options; }
      set
      {
        this.EnsureNotLocked();
        options = value;
      }
    }

    /// <summary>
    /// Gets a value indicating whether session is system.
    /// </summary>
    public bool IsSystem
    {
      get { return (type & SessionType.System)!=0; }
    }

    /// <summary>
    /// Gets a value indicating whether session uses ambient transactions.
    /// </summary>
    public bool UsesAmbientTransactions
    {
      get { return (options & SessionOptions.AmbientTransactions)!=0; }
    }

    /// <inheritdoc/>
    public override void Validate()
    {
      ArgumentValidator.EnsureArgumentIsInRange(CacheSize, 0, Int32.MaxValue, "CacheSize");
    }

    /// <inheritdoc/>
    protected override ConfigurationBase CreateClone()
    {
      var clone = new SessionConfiguration();
      clone.Clone(this);
      return clone;
    }

    /// <inheritdoc/>
    protected override void Clone(ConfigurationBase source)
    {
      base.Clone(source);
      var configuration = (SessionConfiguration) source;
      if ((configuration.type & SessionType.System) > 0)
        throw new InvalidOperationException(Resources.Strings.ExUnableToCloneSystemSessionConfiguration);
      UserName = configuration.UserName;
      Password = configuration.Password;
      Options = configuration.Options;
      CacheType = configuration.CacheType;
      CacheSize = configuration.CacheSize;
      DefaultIsolationLevel = configuration.DefaultIsolationLevel;
    }

    /// <summary>
    /// Clones this instance.
    /// </summary>
    /// <returns>The clone of this configuration.</returns>
    public new SessionConfiguration Clone()
    {
      return (SessionConfiguration) base.Clone();
    }

    /// <inheritdoc/>
    public override string ToString()
    {
      if (UserName.IsNullOrEmpty())
        return string.Format("Name = {0}, Options = {1}, CacheType = {2}, CacheSize = {3}, DefaultIsolationLevel = {4}", Name, Options, CacheType, CacheSize, DefaultIsolationLevel);
      return string.Format("Name = {0}, UserName = {1}, Options = {2}, CacheType = {3}, CacheSize = {4}, DefaultIsolationLevel = {5}", Name, UserName, Options, CacheType, CacheSize, DefaultIsolationLevel);
    }


    // Constructors

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    public SessionConfiguration()
    {
      CacheSize = DefaultCacheSize;
      DefaultIsolationLevel = DefaultIsolationLevelValue;
      Name = "Default";
      UserName = string.Empty;
      Password = string.Empty;
    }

    // Type initializer

    static SessionConfiguration()
    {
      Default = new SessionConfiguration();
      Default.Lock(true);
    }
  }
}