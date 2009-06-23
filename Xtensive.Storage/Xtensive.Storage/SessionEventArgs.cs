// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2009.06.04

using System;
using Xtensive.Core.Internals.DocTemplates;

namespace Xtensive.Storage
{
  /// <summary>
  /// Provides data for <see cref="Domain.SessionOpen"/> event.
  /// </summary>
  public sealed class SessionEventArgs : EventArgs
  {
    /// <summary>
    /// Gets the session.
    /// </summary>
    public Session Session { get; private set; }


    // Constructors

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="session">The session.</param>
    public SessionEventArgs(Session session)
    {
      Session = session;
    }
  }
}