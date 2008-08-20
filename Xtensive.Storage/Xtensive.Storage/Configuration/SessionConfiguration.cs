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
    public const int DefaultCacheSize = 1024;

    /// <see cref="HasStaticDefaultDocTemplate.Default" copy="true" />
    public static readonly SessionConfiguration Default;

    private SessionType type = SessionType.User;

    /// <summary>
    /// Gets or sets the session name.
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// Gets user name to authenticate.
    /// </summary>
    public string UserName { get; set; }

    /// <summary>
    /// Gets password to authenticate.
    /// </summary>
    public string Password { get; set; }

    /// <summary>
    /// Gets custom authentication parameters.
    /// </summary>
    public string CustomAuthParams { get; set; }

    /// <summary>
    /// Gets or sets the size of the session cache. Default value is <see cref="DefaultCacheSize"/>.
    /// </summary>
    public int CacheSize { get; set; }

    /// <summary>
    /// Gets session type.
    /// </summary>
    public SessionType Type
    {
      get { return type; }
      internal set { type = value; }
    }

    /// <summary>
    /// Gets <see langword="true" /> if session is system, otherwise gets <see langword="fals" />.
    /// </summary>
    public bool IsSystem
    {
      get { return (type & SessionType.System) > 0; }
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
      if (configuration.IsSystem)
        throw new InvalidOperationException(Resources.Strings.ExUnableToCloneSystemSessionConfiguration);
      UserName = configuration.UserName;
      Password = configuration.Password;
      CustomAuthParams = configuration.CustomAuthParams;
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
      return Equals(obj.UserName, UserName)
        //TODO: && Equals(obj.AuthParams, AuthParams)
        && obj.CacheSize==CacheSize;
    }

    /// <inheritdoc/>
    public override int GetHashCode()
    {
      unchecked {
        int result = (UserName!=null ? UserName.GetHashCode() : 0);
        //todo: result = (result * 397) ^ (AuthParams != null ? AuthParams.GetHashCode() : 0);
        result = (result * 397) ^ CacheSize;
        return result;
      }
    }

    /// <inheritdoc/>
    public override string ToString()
    {
      if (UserName.IsNullOrEmpty())
        return string.Format("Name = {0}, CacheSize = {1}", Name, CacheSize);
      else
        return string.Format("Name = {0}, CacheSize = {1}, UserName = {2}", Name, CacheSize, UserName);
    }


    // Constructors

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    public SessionConfiguration()
    {
      CacheSize = 1024;
    }

    // Type initializer

    static SessionConfiguration()
    {
      Default = new SessionConfiguration();
      Default.Lock(true);
    }
  }
}