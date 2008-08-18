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
    public readonly static SessionConfiguration Default;

    /// <summary>
    /// Gets or sets the session name.
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// Gets user name to authenticate.
    /// </summary>
    public string UserName { get; set; }

    /// <summary>
    /// Gets or sets the value, indicating whether changed entities should be automatically validated.
    /// Default value is <see langword="true" />.
    /// </summary>
    

    //TODO: Change SessionConfiguration.AuthParams to be serializeble to app.config.
    /// <summary>
    /// Gets authentication params.
    /// </summary>
    public object[] AuthParams { get; private set; }

    /// <summary>
    /// Gets or sets the size of the session cache. Default value is <see cref="DefaultCacheSize"/>.
    /// </summary>
    public int CacheSize { get; set; }

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
      var configuration = (SessionConfiguration)source;
      UserName = configuration.UserName;
      AuthParams = configuration.AuthParams;
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
        && obj.CacheSize == CacheSize;
    }

    /// <inheritdoc/>
    public override int GetHashCode()
    {
      unchecked {
        int result = (UserName != null ? UserName.GetHashCode() : 0);
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
    /// <param name="userName">User name to authenticate (use <see cref="string.Empty"/> to omit authentication).</param>
    /// <param name="authParams">Authentication params (e.g. password).</param>
    public SessionConfiguration(string userName, object[] authParams)
      : this()
    {
      UserName = userName;
      AuthParams = authParams;
    }

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
