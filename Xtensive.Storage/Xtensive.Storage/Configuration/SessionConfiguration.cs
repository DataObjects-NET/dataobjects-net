// Copyright (C) 2007 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2007.08.29

using System;
using Xtensive.Core.Helpers;

namespace Xtensive.Storage.Configuration
{
  [Serializable]
  public class SessionConfiguration : ConfigurationBase
  {
    private string userName;
    private object[] authParams;

    /// <summary>
    /// Gets user name to authenticate.
    /// </summary>
    public string UserName
    {
      get { return userName; }
    }

    /// <summary>
    /// Gets authentication params.
    /// </summary>
    public object[] AuthParams
    {
      get { return authParams; }
    }

    public int TupleCacheSize
    {
      get { throw new NotImplementedException(); }
    }

    /// <inheritdoc/>
    public override void Validate()
    {
    }

    /// <inheritdoc/>
    protected override ConfigurationBase CreateClone()
    {
      return new SessionConfiguration();
    }

    protected override void Clone(ConfigurationBase source)
    {
      base.Clone(source);
      SessionConfiguration configuration = (SessionConfiguration)source;
      userName = configuration.UserName;
      authParams = configuration.AuthParams;
    }


    /// <summary>
    /// Initializes a new instance of the <see cref="SessionConfiguration"/> class.
    /// </summary>
    /// <param name="userName">User name to authenticate (use <see cref="String.Empty"/> to omit authentication).</param>
    /// <param name="authParams">Authentication params (e.g. password).</param>
    public SessionConfiguration(string userName, object[] authParams)
    {
      this.userName = userName;
      this.authParams = authParams;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="SessionConfiguration"/> class.
    /// </summary>
    public SessionConfiguration()
    {
    }
  }
}