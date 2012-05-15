// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2009.06.04

using System;


namespace Xtensive.Orm
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
    /// Initializes a new instance of this class.
    /// </summary>
    /// <param name="session">The session.</param>
    public SessionEventArgs(Session session)
    {
      Session = session;
    }
  }
}