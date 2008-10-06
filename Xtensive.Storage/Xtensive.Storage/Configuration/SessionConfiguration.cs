// Copyright (C) 2007 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2007.08.29

using System;
using Xtensive.Core;
using Xtensive.Core.Helpers;
using Xtensive.Core.Internals.DocTemplates;

namespace Xtensive.Storage.Configuration
{
  /// <summary>
  /// A configuration for the <see cref="Session"/> object.
  /// </summary>
  /// <remarks>
  /// <para id="About"><see cref="HasStaticDefaultDocTemplate" copy="true" /></para>
  /// </remarks>
  [Serializable]
  public class SessionConfiguration : ConfigurationBase
  {
    /// <summary>
    /// Default cache size;
    /// </summary>
    public const int DefaultCacheSize = 10240;

    /// <see cref="HasStaticDefaultDocTemplate.Default" copy="true" />
    public static readonly SessionConfiguration Default;

    private SessionOptions options;
    private SessionType type = SessionType.User;
    private string name;
    private string userName;
    private string password;
    private int cacheSize;

    /// <summary>
    /// Gets or sets the session name.
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
    /// Gets session type.
    /// </summary>
    public SessionType Type
    {
      get { return type; }
      internal set { type = value; }
    }

    /// <summary>
    /// Gets or sets session options.
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
      Options   = configuration.Options;
      UserName  = configuration.UserName;
      Password  = configuration.Password;
      CacheSize = configuration.CacheSize;
    }

    /// <inheritdoc/>
    public override bool Equals(object obj)
    {
      if (ReferenceEquals(null, obj))
        return false;
      if (ReferenceEquals(this, obj))
        return true;
      if (obj.GetType()!=typeof (SessionConfiguration))
        return false;
      return Equals((SessionConfiguration) obj);
    }

    /// <inheritdoc/>
    public bool Equals(SessionConfiguration obj)
    {
      if (ReferenceEquals(null, obj))
        return false;
      if (ReferenceEquals(this, obj))
        return true;
      return 
        obj.UserName==UserName &&
        obj.Options==Options &&
        obj.CacheSize==CacheSize;
    }

    /// <inheritdoc/>
    public override int GetHashCode()
    {
      unchecked {
        int result = (UserName!=null ? UserName.GetHashCode() : 0);
        result = (result * 397) ^ (int) Options;
        result = (result * 397) ^ CacheSize;
        return result;
      }
    }

    /// <inheritdoc/>
    public override string ToString()
    {
      if (UserName.IsNullOrEmpty())
        return string.Format("Name = {0}, Options = {1}, CacheSize = {2}", Name, Options, CacheSize);
      else
        return string.Format("Name = {0}, UserName = {1}, Options = {2}, CacheSize = {3}", Name, UserName, Options, CacheSize);
    }


    // Constructors

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    public SessionConfiguration()
    {
      CacheSize = DefaultCacheSize;
    }

    // Type initializer

    static SessionConfiguration()
    {
      Default = new SessionConfiguration();
      Default.Lock(true);
    }
  }
}