// Copyright (C) 2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexis Kochetov
// Created:    2010.09.09

using System;
using System.Diagnostics;
using Xtensive.Core;
using Xtensive.Storage.Configuration;

namespace Xtensive.Storage
{
  public class UISession : Session
  {
    /// <summary>
    /// Opens new <see cref="UISession"/> with specified <see cref="SessionConfiguration"/>.
    /// </summary>
    /// <param name="domain">The domain.</param>
    /// <returns>
    /// New <see cref="UISession"/> object.
    /// </returns>
    /// <sample><code>
    /// using (UISession.Open(domain)) {
    /// // work with persistent objects here
    /// }
    /// </code></sample>
    /// <seealso cref="Session"/>
    public new static UISession Open(Domain domain)
    {
      return Open(domain, null);
    }

    /// <summary>
    /// Opens new <see cref="UISession"/> with specified <see cref="SessionConfiguration"/>.
    /// </summary>
    /// <param name="domain">The domain.</param>
    /// <param name="configuration">The session configuration.</param>
    /// <returns>
    /// New <see cref="UISession"/> object.
    /// </returns>
    /// <sample><code>
    /// using (UISession.Open(domain, sessionConfiguration)) {
    /// // work with persistent objects here
    /// }
    /// </code></sample>
    /// <seealso cref="Session"/>
    public new static UISession Open(Domain domain, SessionConfiguration configuration)
    {
      ArgumentValidator.EnsureArgumentNotNull(domain, "domain");
      ArgumentValidator.EnsureArgumentNotNull(configuration, "configuration");

      return domain.OpenSession(configuration);
    }

    public void SaveChanges()
    {
      DisconnectedState.ApplyChanges();
    }

    public void CancelChanges()
    {
      DisconnectedState.CancelChanges();
    }

    // Constructors

    internal UISession(Domain domain, SessionConfiguration configuration) 
      : base(domain, configuration, false)
    {
      new DisconnectedState().Attach(this);
      DisconnectedState.Connect();
    }
  }
}