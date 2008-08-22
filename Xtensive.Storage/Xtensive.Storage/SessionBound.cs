// Copyright (C) 2007 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2007.08.10

using System;
using System.Diagnostics;
using Xtensive.Core;
using Xtensive.Core.Internals.DocTemplates;
using Xtensive.Storage.Attributes;

namespace Xtensive.Storage
{
  /// <summary>
  /// Base class for all objects that are bound to the <see cref="Session"/>
  /// instance.
  /// </summary>
  public abstract class SessionBound : 
    IContextBound<Session>
  {
    private Session session;

    /// <summary>
    /// Gets <see cref="Session"/> to which current instance is bound.
    /// </summary>
    [Infrastructure]
    public Session Session {
      [DebuggerStepThrough]
      get { return session; }
      [DebuggerStepThrough]
      internal set { session = value; }
    }

    #region IContextBound<Session> Members

    [Infrastructure]
    Session IContextBound<Session>.Context {
      [DebuggerStepThrough]
      get { return session; }
    }

    #endregion


    // Constructors

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    protected SessionBound() 
      : this(SessionScope.Current.Session)
    {
    }

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="session"><see cref="Xtensive.Storage.Session"/>, to which current instance 
    /// is bound.</param>
    /// <exception cref="ArgumentNullException"><paramref name="session"/> is <see langword="null" />.</exception>
    protected SessionBound(Session session)
    {
      ArgumentValidator.EnsureArgumentNotNull(session, "session");
      this.session = session;
    }
  }
}