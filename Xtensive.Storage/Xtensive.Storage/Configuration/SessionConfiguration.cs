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
  public class SessionConfiguration : ConfigurationSectionBase
  {
    /// <see cref="HasStaticDefaultDocTemplate.Default" copy="true" />
    public readonly static SessionConfiguration Default;

    /// <summary>
    /// Gets user name to authenticate.
    /// </summary>
    public string UserName { get; private set; }

    /// <summary>
    /// Gets authentication params.
    /// </summary>
    public object[] AuthParams { get; private set; }

    /// <summary>
    /// Gets or sets the size of the session cache.
    /// </summary>
    public int CacheSize { get; set; }

    /// <inheritdoc/>
    public override void Validate()
    {
      ArgumentValidator.EnsureArgumentIsInRange(CacheSize, 0, Int32.MaxValue, "CacheSize");
    }

    /// <inheritdoc/>
    protected override ConfigurationSectionBase CreateClone()
    {
      var clone = new SessionConfiguration();
      clone.Clone(this);
      return clone;
    }

    /// <inheritdoc/>
    protected override void Clone(ConfigurationSectionBase source)
    {
      base.Clone(source);
      var configuration = (SessionConfiguration) source;
      UserName = configuration.UserName;
      AuthParams = configuration.AuthParams;
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
