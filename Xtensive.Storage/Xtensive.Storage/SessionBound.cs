// Copyright (C) 2007 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2007.08.10

using System;
using Xtensive.Core;

namespace Xtensive.Storage
{
  /// <summary>
  /// Base class for all objects that are bound to the <see cref="Session"/>
  /// instance.
  /// </summary>
  public abstract class SessionBound: IContextBound<Session>
  {
    private Session session;

    /// <summary>
    /// Gets <see cref="Session"/> to which current instance is bound.
    /// </summary>
    public Session Session
    {
      get { return session; }
      internal set { session = value; }
    }

    #region IContextBound<Session> Members

    Session IContextBound<Session>.Context
    {
      get { return session; }
    }

    #endregion


    // Constructors

    /// <summary>
    /// Initializes a new instance of this class.
    /// </summary>
    protected SessionBound() 
      : this(SessionScope.Current.Session)
    {
    }

    /// <summary>
    /// Initializes a new instance of this class.
    /// </summary>
    /// <param name="session">Session, to which current instance should be bound.</param>
    /// <exception cref="ArgumentNullException">When <paramref name="session"/> is null.</exception>
    protected SessionBound(Session session)
    {
      ArgumentValidator.EnsureArgumentNotNull(session, "session");
      this.session = session;
    }
  }
}